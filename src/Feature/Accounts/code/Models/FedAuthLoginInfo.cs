using System.Collections.Generic;       

namespace Sitecore.HabitatHome.Feature.Accounts.Models
{
    public class FedAuthLoginInfo
    {
        public IEnumerable<FedAuthLoginButton> LoginButtons { get; set; }
    }
}