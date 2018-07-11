using System;
using System.Net.Mail;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace Sitecore.HabitatHome.Feature.Accounts.Services
{
  public interface IAccountsSettingsService
  {
    string GetPageLink(Item contextItem, ID fieldId);

    MailMessage GetForgotPasswordMailTemplate();

    string GetPageLinkOrDefault(Item contextItem, ID field, Item defaultItem = null);

    Guid? GetRegistrationOutcome(Item contextItem);
  }
}