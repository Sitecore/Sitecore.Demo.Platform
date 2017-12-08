namespace Sitecore.Feature.Accounts.Services
{
    using Sitecore.Data;
    using System;
    using System.Net.Mail;

    public interface IAccountsSettingsService
    {
        string GetSettingsPageLink(ID fieldID);
        MailMessage GetForgotPasswordMailTemplate();
        //string GetSettingsPageLink(ID field, Item defaultItem = null);
        Guid? GetRegistrationOutcome();
    }
}