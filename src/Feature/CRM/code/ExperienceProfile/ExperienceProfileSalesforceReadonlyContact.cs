using System;
using System.Collections.Generic;
using System.Data;
using Sitecore.Cintel.ContactService.Model;
using Sitecore.XConnect;

namespace Sitecore.Demo.Platform.Feature.CRM.ExperienceProfile
{
    [Serializable]
    public class ExperienceProfileSalesforceReadonlyContact : ReadonlyContact
    {
        private string _salesforceContactCreatedDate;
        private string _customSalesforceJourneyStatus;

#pragma warning disable CS0618 // Type or member is obsolete
        // TODO: Replace IPhoneNumber by ReadonlyPhoneNumber when upgrading to the next Sitecore version
        public ExperienceProfileSalesforceReadonlyContact(Guid contactId, int classification, List<ContactIdentifier> identifiers, string firstName, string middleName, string surname, string title, string suffix, string nickname, DateTime? birthDate, string gender, string jobTitle, int totalValue, int visitCount, KeyValuePair<string, IAddress> preferredAddress, KeyValuePair<string, IEmailAddress> preferredEmailAddress, KeyValuePair<string, IPhoneNumber> preferredPhoneNumber, IList<KeyValuePair<string, IAddress>> addresses, IList<KeyValuePair<string, IEmailAddress>> emailAddresses, IList<KeyValuePair<string, IPhoneNumber>> phoneNumbers, string salesforceContactCreatedDate, string customSalesforceJourneyStatus)
            : base(contactId, classification, identifiers, firstName, middleName, surname, title, suffix, nickname, birthDate, gender, jobTitle, totalValue, visitCount, preferredAddress, preferredEmailAddress, preferredPhoneNumber, addresses, emailAddresses, phoneNumbers)
        {
            this._salesforceContactCreatedDate = salesforceContactCreatedDate;
            this._customSalesforceJourneyStatus = customSalesforceJourneyStatus;
        }
#pragma warning restore CS0618 // Type or member is obsolete

        public string SalesforceContactCreatedDate
        {
            get
            {
                return this._salesforceContactCreatedDate;
            }
            set
            {
                throw new ReadOnlyException("Readonly Contact is readonly");
            }
        }

        public string CustomSalesforceJourneyStatus
        {
            get
            {
                return this._customSalesforceJourneyStatus;
            }
            set
            {
                throw new ReadOnlyException("Readonly Contact is readonly");
            }
        }
    }
}
