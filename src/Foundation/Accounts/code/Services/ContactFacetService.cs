using Newtonsoft.Json;
using Sitecore.Analytics;
using Sitecore.Analytics.Model;
using Sitecore.Analytics.Tracking;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.HabitatHome.Foundation.Accounts.Models;
using Sitecore.HabitatHome.Foundation.Accounts.Providers;
using Sitecore.HabitatHome.Foundation.DependencyInjection;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Client.Configuration;
using Sitecore.XConnect.Client.Serialization;
using Sitecore.XConnect.Collection.Model;
using System;
using System.Linq;
using System.Net;

namespace Sitecore.HabitatHome.Foundation.Accounts.Services
{
    [Service(typeof(IContactFacetService))]
    public class ContactFacetService : IContactFacetService
    {
        private readonly IContactFacetsProvider contactFacetsProvider;
        private readonly IExportFileService exportFileService;
        private readonly ContactManager contactManager;
        private readonly string[] facetsToUpdate = { PersonalInformation.DefaultFacetKey, AddressList.DefaultFacetKey, EmailAddressList.DefaultFacetKey, ConsentInformation.DefaultFacetKey, PhoneNumberList.DefaultFacetKey, Avatar.DefaultFacetKey, EngagementMeasures.DefaultFacetKey};

        public ContactFacetService(IContactFacetsProvider contactFacetsProvider, IExportFileService exportFileService)
        {
            this.contactFacetsProvider = contactFacetsProvider;
            this.exportFileService = exportFileService;
            this.contactManager = Factory.CreateObject("tracking/contactManager", true) as ContactManager;
        }

        public void UpdateContactFacets(ContactFacetData data)
        {
            var id = this.GetContactId();
            if (id == null)
            {
                return;
            }

            var contactReference = new IdentifiedContactReference(id.Source, id.Identifier);

            using (var client = SitecoreXConnectClientConfiguration.GetClient())
            {
                try
                {
                    var contact = client.Get(contactReference, new ContactExpandOptions(this.facetsToUpdate));
                    if (contact == null)
                    {
                        return;
                    }
                    var changed = false;
                    changed |= SetPersonalInfo(data, contact, client);
                    changed |= this.SetPhone(data, contact, client);
                    changed |= this.SetEmail(data, contact, client);
                    changed |= this.SetAvatar(data, contact, client);

                    if (changed)
                    {
                        client.Submit();
                        this.UpdateTracker();
                    }
                }
                catch (XdbExecutionException ex)
                {
                    Log.Error($"Could not update the xConnect contact facets", ex, this);
                }
            }
        }

        public string ExportContactData()
        {
            var id = this.GetContactId();
            if (id == null)
            {
                return string.Empty;
            }

            var contactReference = new IdentifiedContactReference(id.Source, id.Identifier);

            using (var client = SitecoreXConnectClientConfiguration.GetClient())
            {
                try
                {
                    var contactFacets = client.Model.Facets.Where(c => c.Target == EntityType.Contact).Select(x => x.Name);

                    var interactionFacets = client.Model.Facets.Where(c => c.Target == EntityType.Interaction).Select(x => x.Name);

                    var contact = client.Get(contactReference, new ContactExpandOptions(contactFacets.ToArray())
                    {
                        Interactions = new RelatedInteractionsExpandOptions(interactionFacets.ToArray())
                        {
                            EndDateTime = DateTime.MaxValue,
                            StartDateTime = DateTime.MinValue
                        }
                    });

                    if (contact == null)
                    {
                        return string.Empty;
                    }

                    var serializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new XdbJsonContractResolver(client.Model,
                            serializeFacets: true,
                            serializeContactInteractions: true),
                        Formatting = Formatting.Indented,
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    };

                    string exportedData = JsonConvert.SerializeObject(contact, serializerSettings);

                    var fileWithExportedData = exportFileService.CreateExportFile();

                    exportFileService.WriteExportedDataIntoFile(fileWithExportedData, exportedData);

                    return fileWithExportedData;
                }
                catch (XdbExecutionException ex)
                {
                    Log.Error($"Could not export the xConnect contact & interaction data", ex, this);
                    return string.Empty;
                }
            }
        }

        public bool DeleteContact()
        {
            var id = this.GetContactId();
            if (id == null)
            {
                return false;
            }

            var contactReference = new IdentifiedContactReference(id.Source, id.Identifier);

            using (var client = SitecoreXConnectClientConfiguration.GetClient())
            {
                try
                {
                    var contact = client.Get(contactReference, new Sitecore.XConnect.ContactExpandOptions() { });

                    if (contact == null)
                    {
                        return false;
                    }

                    client.ExecuteRightToBeForgotten(contact);

                    client.Submit();

                    return true;
                }
                catch (XdbExecutionException ex)
                {
                    Log.Error($"Could not delete the xConnect contact", ex, this);
                    return false;
                }
            }
        }

        private bool SetAvatar(ContactFacetData data, XConnect.Contact contact, XConnectClient client)
        {
            var url = data.AvatarUrl;
            var mimeType = data.AvatarMimeType;
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(mimeType))
            {
                return false;
            }
            try
            {
                var web = new WebClient();
                var pictureData = web.DownloadData(url);
                var pictureMimeType = mimeType;

                var avatar = contact.Avatar();
                if (avatar == null)
                {
                    avatar = new Avatar(pictureMimeType, pictureData);
                }
                else
                {
                    avatar.MimeType = pictureMimeType;
                    avatar.Picture = pictureData;
                }
                client.SetFacet(contact, Avatar.DefaultFacetKey, avatar);
                return true;
            }
            catch (Exception exception)
            {
                Log.Warn($"Could not download profile picture {url}", exception, this);
                return false;
            }
        }

        private bool SetEmail(ContactFacetData data, XConnect.Contact contact, XConnectClient client)
        {
            var email = data.EmailAddress;
            var emailKey = data.EmailKey;
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            if (contact.Emails() != null)
            {
                contact.Emails().PreferredEmail = new EmailAddress(email, true);
                contact.Emails().PreferredKey = emailKey;
                client.SetFacet(contact, EmailAddressList.DefaultFacetKey, contact.Emails());
            }
            else
            {
                client.SetFacet(contact, EmailAddressList.DefaultFacetKey, new EmailAddressList(new EmailAddress(email, true), emailKey));
            }

            return true;
        }

        private bool SetPhone(ContactFacetData data, XConnect.Contact contact, XConnectClient client)
        {
            var phoneNumber = data.PhoneNumber;
            var phoneKey = data.PhoneKey;
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return false;
            }

            if (contact.PhoneNumbers() != null)
            {
                contact.PhoneNumbers().PreferredPhoneNumber = new Sitecore.XConnect.Collection.Model.PhoneNumber(string.Empty, phoneNumber);
                contact.PhoneNumbers().PreferredKey = phoneKey;
                client.SetFacet(contact, PhoneNumberList.DefaultFacetKey, contact.PhoneNumbers());
            }
            else
            {
                client.SetFacet(contact, PhoneNumberList.DefaultFacetKey, new PhoneNumberList(new PhoneNumber(string.Empty, phoneNumber), phoneKey));
            }

            return true;
        }

        private static bool SetPersonalInfo(ContactFacetData data, XConnect.Contact contact, XConnectClient client)
        {
            var changed = false;

            var personalInfo = contact.Personal() ?? new PersonalInformation();
            changed |= SetBirthdate(data, personalInfo);
            changed |= SetName(data, personalInfo);
            changed |= SetGender(data, personalInfo);
            changed |= SetLanguage(data, personalInfo);
            if (!changed)
            {
                return false;
            }
            client.SetFacet(contact, PersonalInformation.DefaultFacetKey, personalInfo);
            return true;
        }

        private static bool SetLanguage(ContactFacetData data, PersonalInformation personalInfo)
        {
            if (personalInfo.PreferredLanguage == data.Language)
            {
                return false;
            }
            personalInfo.PreferredLanguage = data.Language;
            return true;
        }

        private static bool SetGender(ContactFacetData data, PersonalInformation personalInfo)
        {
            if (personalInfo.Gender == data.Gender)
            {
                return false;
            }
            personalInfo.Gender = data.Gender;
            return true;
        }

        private static bool SetName(ContactFacetData data, PersonalInformation personalInfo)
        {
            var changed = false;
            if (personalInfo.FirstName != data.FirstName)
            {
                personalInfo.FirstName = data.FirstName;
                changed = true;
            }
            if (personalInfo.MiddleName != data.MiddleName)
            {
                personalInfo.MiddleName = data.MiddleName;
                changed = true;
            }
            if (personalInfo.LastName != data.LastName)
            {
                personalInfo.LastName = data.LastName;
                changed = true;
            }
            return changed;
        }

        private static bool SetBirthdate(ContactFacetData data, PersonalInformation personalInfo)
        {
            var birthDateString = data.Birthday;
            if (string.IsNullOrEmpty(birthDateString))
            {
                return false;
            }
            DateTime birthDate;
            if (!DateTime.TryParse(birthDateString, out birthDate))
            {
                return false;
            }
            personalInfo.Birthdate = birthDate;
            return true;
        }

        private Analytics.Model.Entities.ContactIdentifier GetContactId()
        {
            if (Tracker.Current?.Contact == null)
            {
                return null;
            }
            if (Tracker.Current.Contact.IsNew)
            {
                Tracker.Current.Contact.ContactSaveMode = ContactSaveMode.AlwaysSave;
                this.contactManager.SaveContactToCollectionDb(Tracker.Current.Contact);
            }
            return Tracker.Current.Contact.Identifiers.FirstOrDefault();
        }

        private void UpdateTracker()
        {
            if (Tracker.Current?.Session == null)
            {
                return;
            }
            Tracker.Current.Session.Contact = this.contactManager.LoadContact(Tracker.Current.Session.Contact.ContactId);
        }
    }
}