using System.Collections.Generic;

namespace Sitecore.Demo.Platform.Feature.Accounts.Models
{
    public class FedAuthLoginInfo
    {
        public IEnumerable<FedAuthLoginButton> LoginButtons { get; set; }
    }
}