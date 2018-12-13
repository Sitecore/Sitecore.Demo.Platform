using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.MicrosoftAccount;
using Sitecore.Diagnostics;
using Sitecore.Owin.Authentication.Configuration;
using Sitecore.Owin.Authentication.Extensions;
using Sitecore.Owin.Authentication.Pipelines.IdentityProviders;
using Sitecore.Owin.Authentication.Services;
using System.Threading.Tasks;
using Sitecore.Abstractions;
using MicrosoftAccountAuthenticationExtensions = Owin.MicrosoftAccountAuthenticationExtensions;

namespace Sitecore.HabitatHome.Feature.Accounts.Infrastructure.Pipelines.IdentityProviders
{        
    public class MicrosoftAccount : IdentityProvidersProcessor
    {
        public MicrosoftAccount(FederatedAuthenticationConfiguration federatedAuthenticationConfiguration, ICookieManager cookieManager, BaseSettings baseSettings) 
            : base(federatedAuthenticationConfiguration, cookieManager, baseSettings)
        {
        }

        protected override string IdentityProviderName => "MicrosoftAccount";

        protected override void ProcessCore([NotNull] IdentityProvidersArgs args)
        {      
            Assert.ArgumentNotNull(args, nameof(args));                                       

            var identityProvider = this.GetIdentityProvider();
            var authenticationType = this.GetAuthenticationType();
                                                                                                             
            string clientSecret = Settings.GetSetting("Sitecore.HabitatHome.Feature.Accounts.Microsoft.ClientSecret");     //todo: move this to site-specific configuration item   
            string clientId = Settings.GetSetting("Sitecore.HabitatHome.Feature.Accounts.Microsoft.ClientId"); //todo: move this to site-specific configuration item        
                
            if(string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(clientId))
            {
                return;
            }

            var options = new MicrosoftAccountAuthenticationOptions
            {                                
                Caption = identityProvider.Caption,
                AuthenticationType = authenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                ClientId = clientId,    
                ClientSecret = clientSecret,
                Provider = new MicrosoftAccountAuthenticationProvider()
                {
                    OnAuthenticated = (context) =>
                      Task.Run(() =>
                      {
                          context.Identity.ApplyClaimsTransformations(new TransformationContext(this.FederatedAuthenticationConfiguration, identityProvider));
                      })
                }
            };

            MicrosoftAccountAuthenticationExtensions.UseMicrosoftAccountAuthentication(args.App, options);                               
        }           
    }
}