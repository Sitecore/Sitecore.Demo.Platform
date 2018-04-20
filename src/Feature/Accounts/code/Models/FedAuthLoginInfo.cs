using System;
using System.Collections.Generic;       

namespace Sitecore.Feature.Accounts.Models
{
    public class FedAuthLoginInfo
    {
        public IEnumerable<FedAuthLoginButton> LoginButtons { get; set; }
    }
}