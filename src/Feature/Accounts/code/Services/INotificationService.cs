namespace Sitecore.Demo.Platform.Feature.Accounts.Services
{
    public interface INotificationService
    {
        void SendPassword(string email, string newPassword);
    }
}