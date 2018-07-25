using System.Collections.Generic;
using Sitecore.HabitatHome.Feature.Accounts.Models;      

namespace Sitecore.HabitatHome.Feature.Accounts.Repositories
{
    public interface IFedAuthLoginButtonRepository
    {
        IEnumerable<FedAuthLoginButton> GetAll();
    }
}