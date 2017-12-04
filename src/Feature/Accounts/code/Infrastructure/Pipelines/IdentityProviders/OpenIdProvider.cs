using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenIdConnectAuthenticationExtensions = Owin.OpenIdConnectAuthenticationExtensions;


namespace Sitecore.Feature.Accounts.Infrastructure.Pipelines.IdentityProviders
{
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.OpenIdConnect;
    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Sitecore.Owin.Authentication.Configuration;
    using Sitecore.Owin.Authentication.Extensions;
    using Sitecore.Owin.Authentication.Pipelines.IdentityProviders;
    using Sitecore.Owin.Authentication.Services;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class OpenIdProvider : IdentityProvidersProcessor
    {
        public OpenIdProvider(FederatedAuthenticationConfiguration federatedAuthenticationConfiguration) : base(federatedAuthenticationConfiguration)
        {
        }

        protected override string IdentityProviderName => "OpenId";

        protected override void ProcessCore([NotNull] IdentityProvidersArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));

            var identityProvider = this.GetIdentityProvider();
            var authenticationType = this.GetAuthenticationType();
                                                                                                             
            string authority = Settings.GetSetting("Sitecore.Feature.Accounts.Microsoft.Authority");     //todo: move this to site-specific configuration item 
            string clientId = Settings.GetSetting("Sitecore.Feature.Accounts.Microsoft.ClientId"); //todo: move this to site-specific configuration item 
            string postLogoutRedirectURI = "/"; //todo: move this to site-specific configuration item
            string redirectURI = "/";  //todo: move this to site-specific configuration item                                                                                                     

            var options = new OpenIdConnectAuthenticationOptions
            {
                Caption = identityProvider.Caption,
                AuthenticationType = authenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                ClientId = clientId,                      
                Authority = authority,                   
                RedirectUri = redirectURI,                
                PostLogoutRedirectUri = postLogoutRedirectURI,            
                Notifications = new OpenIdConnectAuthenticationNotifications
                {                                                       
                    SecurityTokenValidated = notification =>
                    {
                        var identity = notification.AuthenticationTicket.Identity;

                        foreach (var claimTransformationService in identityProvider.Transformations)
                        {
                            claimTransformationService.Transform(identity, new TransformationContext
                            {
                                IdentityProvider = identityProvider
                            });
                        }
                        notification.AuthenticationTicket = new AuthenticationTicket(identity, notification.AuthenticationTicket.Properties);

                        return Task.CompletedTask;
                    }
                }   
            };

            OpenIdConnectAuthenticationExtensions.UseOpenIdConnectAuthentication(args.App, options);                               
        }                               
    }
}