using System;

namespace Sitecore.HabitatHome.Feature.Accounts.Services
{
    public interface INotificationService
    {
        void SendPassword(string email, string newPassword);
    }
}