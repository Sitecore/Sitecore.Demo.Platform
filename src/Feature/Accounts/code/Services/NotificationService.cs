using Sitecore.HabitatHome.Foundation.DependencyInjection;

namespace Sitecore.HabitatHome.Feature.Accounts.Services
{
    [Service(typeof(INotificationService))]
    public class NotificationService : INotificationService
    {
        private readonly IAccountsSettingsService _siteSettings;

        public NotificationService(IAccountsSettingsService siteSettings)
        {
            this._siteSettings = siteSettings;
        }

        public void SendPassword(string email, string newPassword)
        {
            var mail = this._siteSettings.GetForgotPasswordMailTemplate();
            mail.To.Add(email);
            mail.Body = mail.Body.Replace("$password$", newPassword);

            MainUtil.SendMail(mail);
        }
    }
}