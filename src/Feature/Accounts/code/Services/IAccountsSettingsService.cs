namespace Sitecore.Feature.Accounts.Services
{
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using System;
    using System.Net.Mail;

    public interface IAccountsSettingsService
    {
        string GetPageLink(Item contextItem, ID fieldID);
        MailMessage GetForgotPasswordMailTemplate();
        string GetPageLinkOrDefault(Item contextItem, ID field, Item defaultItem = null);
        Guid? GetRegistrationOutcome(Item contextItem);
    }
}