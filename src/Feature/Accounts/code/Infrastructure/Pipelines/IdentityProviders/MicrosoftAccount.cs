using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MicrosoftAccountAuthenticationExtensions = Owin.MicrosoftAccountAuthenticationExtensions;


namespace Sitecore.Feature.Accounts.Infrastructure.Pipelines.IdentityProviders
{
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.MicrosoftAccount;
    using Microsoft.Owin.Security.Notifications;
    using Microsoft.IdentityModel.Protocols;
    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Sitecore.Owin.Authentication.Configuration;
    using Sitecore.Owin.Authentication.Extensions;
    using Sitecore.Owin.Authentication.Pipelines.IdentityProviders;
    using Sitecore.Owin.Authentication.Services;
    using System.Security.Claims;
    using System.Threading.Tasks;                     
    using Microsoft.Owin.Security.Cookies;             

    public class MicrosoftAccount : IdentityProvidersProcessor
    {
        public MicrosoftAccount(FederatedAuthenticationConfiguration federatedAuthenticationConfiguration) : base(federatedAuthenticationConfiguration)
        {
        }

        protected override string IdentityProviderName => "MicrosoftAccount";

        protected override void ProcessCore([NotNull] IdentityProvidersArgs args)
        {      
            Assert.ArgumentNotNull(args, nameof(args));                                       

            var identityProvider = this.GetIdentityProvider();
            var authenticationType = this.GetAuthenticationType();
                                                                                                             
            string clientSecret = Settings.GetSetting("Sitecore.Feature.Accounts.Microsoft.ClientSecret");     //todo: move this to site-specific configuration item   
            string clientId = Settings.GetSetting("Sitecore.Feature.Accounts.Microsoft.ClientId"); //todo: move this to site-specific configuration item        
                
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