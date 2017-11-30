namespace Sitecore.Feature.Accounts.Repositories
{
    using Sitecore.Feature.Accounts.Models;
    using System.Collections.Generic;

    public interface IFedAuthLoginButtonRepository
    {
        IEnumerable<FedAuthLoginButton> GetAll();
    }
}