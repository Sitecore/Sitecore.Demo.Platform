using System.Collections.Generic;
using Sitecore.Demo.Platform.Feature.Accounts.Models;

namespace Sitecore.Demo.Platform.Feature.Accounts.Repositories
{
    public interface IFedAuthLoginButtonRepository
    {
        IEnumerable<FedAuthLoginButton> GetAll();
    }
}