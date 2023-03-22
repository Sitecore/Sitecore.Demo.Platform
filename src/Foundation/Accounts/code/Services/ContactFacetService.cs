﻿using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Sitecore.Analytics;
using Sitecore.Analytics.Model;
using Sitecore.Demo.Platform.Foundation.Accounts.Models;
using Sitecore.Demo.Platform.Foundation.Accounts.Providers;
using Sitecore.Demo.Platform.Foundation.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Client.Configuration;
using Sitecore.XConnect.Client.Serialization;
using Sitecore.XConnect.Collection.Model;

namespace Sitecore.Demo.Platform.Foundation.Accounts.Services
{
    [Service(typeof(IContactFacetService))]
    public class ContactFacetService : IContactFacetService
    {
        private readonly IContactFacetsProvider contactFacetsProvider;
        private readonly IExportFileService exportFileService;
        private readonly string[] facetsToUpdate = {
            PersonalInformation.DefaultFacetKey,
            AddressList.DefaultFacetKey,
            EmailAddressList.DefaultFacetKey,
            ConsentInformation.DefaultFacetKey,
            PhoneNumberList.DefaultFacetKey,
            Avatar.DefaultFacetKey,
            EngagementMeasures.DefaultFacetKey
        };

        public ContactFacetService(IContactFacetsProvider contactFacetsProvider, IExportFileService exportFileService)
        {
            this.contactFacetsProvider = contactFacetsProvider;
            this.exportFileService = exportFileService;
        }

        public ContactFacetData GetContactData()
        {
            ContactFacetData data = new ContactFacetData();

            var contactReference = this.GetContactId();

            if (contactReference != null)
            {
                using (var client = SitecoreXConnectClientConfiguration.GetClient())
                {
                    try
                    {
                        var contact = client.Get(contactReference, new ContactExecutionOptions(new ContactExpandOptions(this.facetsToUpdate)));

                        if (contact != null)
                        {
                            PersonalInformation personalInformation = contact.Personal();
                            if (personalInformation != null)
                            {
                                data.FirstName = personalInformation.FirstName;
                                data.MiddleName = personalInformation.MiddleName;
                                data.LastName = personalInformation.LastName;
                                data.Birthday = personalInformation.Birthdate.ToString();
                                data.Gender = personalInformation.Gender;
                                data.Language = personalInformation.PreferredLanguage;
                            }

                            var email = contact.Emails();
                            if (email != null)
                            {
                                data.EmailAddress = email.PreferredEmail?.SmtpAddress;
                            }

                            var phones = contact.PhoneNumbers();
                            if (phones != null)
                            {
                                data.PhoneNumber = phones.PreferredPhoneNumber?.Number;
                            }
                        }
                    }
                    catch (XdbExecutionException ex)
                    {
                        Log.Error($"Could not get the xConnect contact facets", ex, this);
                    }
                }
            }

            return data;
        }

        public void UpdateContactFacets(ContactFacetData data)
        {
            var contactReference = this.GetContactId();
            if (contactReference == null)
            {
                return;
            }

            using (var client = SitecoreXConnectClientConfiguration.GetClient())
            {
                try
                {
                    var contact = client.Get(contactReference, new ContactExecutionOptions(new ContactExpandOptions(this.facetsToUpdate)));
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
            var contactReference = this.GetContactId();
            if (contactReference == null)
            {
                return string.Empty;
            }

            using (var client = SitecoreXConnectClientConfiguration.GetClient())
            {
                try
                {
                    var contactFacets = client.Model.Facets.Where(c => c.Target == EntityType.Contact).Select(x => x.Name);

                    var interactionFacets = client.Model.Facets.Where(c => c.Target == EntityType.Interaction).Select(x => x.Name);

                    var contact = client.Get(contactReference, new ContactExecutionOptions(new ContactExpandOptions(contactFacets.ToArray())
                    {
                        Interactions = new RelatedInteractionsExpandOptions(interactionFacets.ToArray())
                        {
                            EndDateTime = DateTime.MaxValue,
                            StartDateTime = DateTime.MinValue
                        }
                    }));

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
            var contactReference = this.GetContactId();
            if (contactReference == null)
            {
                return false;
            }

            using (var client = SitecoreXConnectClientConfiguration.GetClient())
            {
                try
                {
                    var contact = client.Get(contactReference, new ContactExecutionOptions(new ContactExpandOptions()));

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

        private bool SetAvatar(ContactFacetData data, Contact contact, XConnectClient client)
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

        private bool SetEmail(ContactFacetData data, Contact contact, XConnectClient client)
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

        private bool SetPhone(ContactFacetData data, Contact contact, XConnectClient client)
        {
            var phoneNumber = data.PhoneNumber;
            var phoneKey = data.PhoneKey;
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return false;
            }

            if (contact.PhoneNumbers() != null)
            {
                contact.PhoneNumbers().PreferredPhoneNumber = new PhoneNumber(string.Empty, phoneNumber);
                contact.PhoneNumbers().PreferredKey = phoneKey;
                client.SetFacet(contact, PhoneNumberList.DefaultFacetKey, contact.PhoneNumbers());
            }
            else
            {
                client.SetFacet(contact, PhoneNumberList.DefaultFacetKey, new PhoneNumberList(new PhoneNumber(string.Empty, phoneNumber), phoneKey));
            }

            return true;
        }

        private static bool SetPersonalInfo(ContactFacetData data, Contact contact, XConnectClient client)
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
            if (personalInfo.FirstName != data.FirstName && !string.IsNullOrWhiteSpace(data.FirstName))
            {
                personalInfo.FirstName = data.FirstName;
                changed = true;
            }
            if (personalInfo.MiddleName != data.MiddleName && !string.IsNullOrWhiteSpace(data.MiddleName))
            {
                personalInfo.MiddleName = data.MiddleName;
                changed = true;
            }
            if (personalInfo.LastName != data.LastName && !string.IsNullOrWhiteSpace(data.LastName))
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

        public IdentifiedContactReference GetContactId()
        {
            if (Tracker.Current?.Contact == null)
            {
                return null;
            }

            var trackerIdentifier = new IdentifiedContactReference(Analytics.XConnect.DataAccess.Constants.IdentifierSource, Tracker.Current.Contact.ContactId.ToString("N"));

            if (Tracker.Current.Contact.IsNew)
            {
                Tracker.Current.Contact.ContactSaveMode = ContactSaveMode.AlwaysSave;
                using (var client = SitecoreXConnectClientConfiguration.GetClient())
                {
                    client.Submit();
                }

                return trackerIdentifier;
            }

            var id = Tracker.Current.Contact.Identifiers.FirstOrDefault();
            if (id == null)
            {
                return trackerIdentifier;
            }

            return new IdentifiedContactReference(id.Source, id.Identifier);
        }
    }
}
