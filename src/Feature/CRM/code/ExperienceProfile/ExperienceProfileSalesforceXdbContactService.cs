using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Cintel.ContactService;
using Sitecore.Cintel.ContactService.Model;
using Sitecore.Cintel.Diagnostics;
using Sitecore.DataExchange.Tools.SalesforceConnect.Facets;
using Sitecore.Demo.Platform.Feature.CRM.CustomCollectionModels;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Client.Configuration;
using Sitecore.XConnect.Collection.Model;

namespace Sitecore.Demo.Platform.Feature.CRM.ExperienceProfile
{
    public class ExperienceProfileSalesforceXdbContactService : XdbContactService
    {
        public new IContact Get(Guid contactId)
        {
            string[] facets = new string[8]
            {
                "Personal",
                "Addresses",
                "Emails",
                "PhoneNumbers",
                "Classification",
                "EngagementMeasures",
                "SalesforceContact",
                "CustomSalesforceContact"
            };
            Contact contact = this.GetContact(contactId, facets);
            PersonalInformation facet1 = this.TryGetFacet<PersonalInformation>(contact, "Personal");
            AddressList facet2 = this.TryGetFacet<AddressList>(contact, "Addresses");
            EmailAddressList facet3 = this.TryGetFacet<EmailAddressList>(contact, "Emails");
            PhoneNumberList facet4 = this.TryGetFacet<PhoneNumberList>(contact, "PhoneNumbers");
            Classification facet5 = this.TryGetFacet<Classification>(contact, "Classification");
            EngagementMeasures facet6 = this.TryGetFacet<EngagementMeasures>(contact, "EngagementMeasures");
            SalesforceContactInformation facet7 = this.TryGetFacet<SalesforceContactInformation>(contact, "SalesforceContact");
            CustomSalesforceContactInformation facet8 = this.TryGetFacet<CustomSalesforceContactInformation>(contact, "CustomSalesforceContact");
            return this.CreateContact(contact.Id.GetValueOrDefault(), facet5, facet6, facet1, facet3, facet4, facet2, facet7, facet8, contact.Identifiers.ToList());
        }

        public Contact GetContact(Guid contactId, string[] facets)
        {
            using (XConnectClient client = SitecoreXConnectClientConfiguration.GetClient("xconnect/clientconfig"))
            {
                ContactReference contactReference = new ContactReference(contactId);
                Contact contact = facets == null || facets.Length == 0 ? client.Get(contactReference, new ContactExecutionOptions(new ContactExpandOptions(Array.Empty<string>()))) : client.Get(contactReference, new ContactExecutionOptions(new ContactExpandOptions(facets)));
                if (contact == null)
                {
                    throw new ContactNotFoundException(string.Format("No Contact with id [{0}] found", contactId));
                }
                return contact;
            }
        }

        public TFacet TryGetFacet<TFacet>(Contact contact, string facetName) where TFacet : Facet
        {
            try
            {
                return contact.GetFacet<TFacet>(facetName);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Could not load facet with name [{0}]", facetName), ex);
            }
            return default(TFacet);
        }

        public ReadonlyContact CreateContact(Guid contactId, Classification classification, EngagementMeasures engagementMeasures, PersonalInformation personalInfo, EmailAddressList emailAddressList, PhoneNumberList phoneNumberList, AddressList addressList, SalesforceContactInformation salseforceContactInformation, CustomSalesforceContactInformation customSalseforceContactInformation, List<ContactIdentifier> identifiers)
        {
            int classification1 = 0;
            int visitCount = 0;
            int totalValue = 0;
            string firstName = string.Empty;
            string middleName = string.Empty;
            string surname = string.Empty;
            string title = string.Empty;
            string suffix = string.Empty;
            string nickname = string.Empty;
            DateTime? birthDate = new DateTime?();
            string gender = string.Empty;
            string jobTitle = string.Empty;
            string salesforceContactCreatedDate = string.Empty;
            string customSalesforceMemberStatus = string.Empty;
            KeyValuePair<string, IAddress> preferredAddress = new KeyValuePair<string, IAddress>();
            KeyValuePair<string, IEmailAddress> preferredEmailAddress = new KeyValuePair<string, IEmailAddress>();
#pragma warning disable CS0618 // Type or member is obsolete
            // TODO: Replace IPhoneNumber by ReadonlyPhoneNumber when upgrading to the next Sitecore version
            KeyValuePair<string, IPhoneNumber> preferredPhoneNumber = new KeyValuePair<string, IPhoneNumber>();
            IList<KeyValuePair<string, IPhoneNumber>> phoneNumbers = new List<KeyValuePair<string, IPhoneNumber>>();
#pragma warning restore CS0618 // Type or member is obsolete
            IList<KeyValuePair<string, IAddress>> addresses = new List<KeyValuePair<string, IAddress>>();
            IList<KeyValuePair<string, IEmailAddress>> emailAddresses = new List<KeyValuePair<string, IEmailAddress>>();
            if (classification != null)
                classification1 = classification.OverrideClassificationLevel > 0 ? classification.OverrideClassificationLevel : classification.ClassificationLevel;
            if (engagementMeasures != null)
            {
                visitCount = engagementMeasures.TotalInteractionCount;
                totalValue = engagementMeasures.TotalValue;
            }
            List<ContactIdentifier> identifiers1 = identifiers != null ? identifiers : new List<ContactIdentifier>();
            if (personalInfo != null)
            {
                firstName = personalInfo.FirstName;
                middleName = personalInfo.MiddleName;
                surname = personalInfo.LastName;
                gender = personalInfo.Gender;
                birthDate = personalInfo.Birthdate;
                jobTitle = personalInfo.JobTitle;
                nickname = personalInfo.Nickname;
                suffix = personalInfo.Suffix;
                title = personalInfo.Title;
            }
            if (emailAddressList != null)
            {
                preferredEmailAddress = emailAddressList.PreferredEmail == null || string.IsNullOrEmpty(emailAddressList.PreferredEmail.SmtpAddress) ? new KeyValuePair<string, IEmailAddress>(string.Empty, null) : new KeyValuePair<string, IEmailAddress>(emailAddressList.PreferredKey, new ReadonlyEmailAddress(emailAddressList.PreferredEmail));
                emailAddresses = emailAddressList.Others.Keys.Select(key => new KeyValuePair<string, IEmailAddress>(key, new ReadonlyEmailAddress(emailAddressList.Others[key]))).OrderBy(kvp => kvp.Key).ToList();
            }
            if (addressList != null)
            {
                preferredAddress = addressList.PreferredAddress == null || string.IsNullOrEmpty(addressList.PreferredKey) ? new KeyValuePair<string, IAddress>(string.Empty, null) : new KeyValuePair<string, IAddress>(addressList.PreferredKey, new ReadonlyAddress(addressList.PreferredAddress));
                addresses = addressList.Others.Keys.Select(key => new KeyValuePair<string, IAddress>(key, new ReadonlyAddress(addressList.Others[key]))).OrderBy(kvp => kvp.Key).ToList();
            }
            if (phoneNumberList != null)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                preferredPhoneNumber = phoneNumberList.PreferredPhoneNumber == null || string.IsNullOrEmpty(phoneNumberList.PreferredKey) ? new KeyValuePair<string, IPhoneNumber>(string.Empty, null) : new KeyValuePair<string, IPhoneNumber>(phoneNumberList.PreferredKey, new ReadonlyPhoneNumber(phoneNumberList.PreferredPhoneNumber));
                phoneNumbers = phoneNumberList.Others.Keys.Select(key => new KeyValuePair<string, IPhoneNumber>(key, new ReadonlyPhoneNumber(phoneNumberList.Others[key]))).OrderBy(kvp => kvp.Key).ToList();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            if (salseforceContactInformation != null)
            {
                salesforceContactCreatedDate = salseforceContactInformation.LastModified.Value.ToLongDateString();
            }
            if (customSalseforceContactInformation != null)
            {
                customSalesforceMemberStatus = customSalseforceContactInformation.MemberTier;
            }
            return new ExperienceProfileSalesforceReadonlyContact(contactId, classification1, identifiers1, firstName, middleName, surname, title, suffix, nickname, birthDate, gender, jobTitle, totalValue, visitCount, preferredAddress, preferredEmailAddress, preferredPhoneNumber, addresses, emailAddresses, phoneNumbers, salesforceContactCreatedDate, customSalesforceMemberStatus);
        }
    }
}
