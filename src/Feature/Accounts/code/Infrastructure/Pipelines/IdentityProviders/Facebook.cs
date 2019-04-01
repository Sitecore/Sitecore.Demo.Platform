using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Facebook;
using Sitecore.Diagnostics;
using Sitecore.Owin.Authentication.Configuration;
using Sitecore.Owin.Authentication.Extensions;
using Sitecore.Owin.Authentication.Pipelines.IdentityProviders;
using Sitecore.Owin.Authentication.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Infrastructure;
using Sitecore.Abstractions;
using FacebookAuthenticationExtensions = Owin.FacebookAuthenticationExtensions;

namespace Sitecore.HabitatHome.Feature.Accounts.Infrastructure.Pipelines.IdentityProviders
{
    public class Facebook : IdentityProvidersProcessor
    {
        public Facebook(FederatedAuthenticationConfiguration federatedAuthenticationConfiguration, ICookieManager cookieManager, BaseSettings baseSettings) : base(federatedAuthenticationConfiguration, cookieManager, baseSettings)
        {
        }

        protected override string IdentityProviderName => "Facebook";

        protected override void ProcessCore([NotNull] IdentityProvidersArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));                               

            var identityProvider = GetIdentityProvider();
            var authenticationType = GetAuthenticationType();

            string appId = Settings.GetSetting("Sitecore.HabitatHome.Feature.Accounts.Facebook.AppId");   //todo: move this to site-specific configuration item
            string appSecret = Settings.GetSetting("Sitecore.HabitatHome.Feature.Accounts.Facebook.AppSecret");  //todo: move this to site-specific configuration item

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
            {
                return;
            }

            var options = new FacebookAuthenticationOptions
            {
                Caption = identityProvider.Caption,
                AuthenticationType = authenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                AppId = appId,
                AppSecret = appSecret,
                Provider = new FacebookAuthenticationProvider
                {
                    OnAuthenticated = context =>
                    {
                        context.Identity.ApplyClaimsTransformations(new TransformationContext(this.FederatedAuthenticationConfiguration, identityProvider));
                        AddClaim(context, "first_name", "first_name");
                        AddClaim(context, "middle_name", "middle_name");
                        AddClaim(context, "last_name", "last_name");
                        AddClaim(context, "full_name", "name");
                        AddClaim(context, "gender", "gender");
                        AddClaim(context, "birthday", "birthday");
                        AddClaim(context, "link", "link");
                        AddClaim(context, "locale", "locale");
                        AddClaim(context, "location", "location");
                        AddClaim(context, "picture", "picture");
                        AddClaim(context, "timezone", "timezone");
                        return Task.CompletedTask;
                    }
                },
                Scope =
                {
                    "public_profile",
                    "email"
                },
                Fields =
                {
                    "first_name",
                    "middle_name",
                    "last_name",
                    "name",
                    "email",
                    "age_range",
                    "link",
                    "gender",
                    "locale",
                    "location",
                    "picture",
                    "timezone",
                    "updated_time",
                    "verified"
                }
            };

            FacebookAuthenticationExtensions.UseFacebookAuthentication(args.App, options);
        }

        private void AddClaim(FacebookAuthenticatedContext context, string claimName, string propertyName)
        {
            var value = context.User[propertyName]?.ToString();
            if (propertyName == "picture")
            {
                value = context.User["picture"]?["data"]?["url"]?.ToString();
                if (value == null)
                {
                    return;
                }

                context.Identity.AddClaim(new Claim("picture_url", value));
                context.Identity.AddClaim(new Claim("picture_mime", "image/jpg"));

                return;
            }

            if (value == null)
            {
                return;
            }

            context.Identity.AddClaim(new Claim(claimName, value));
        }
    }
}