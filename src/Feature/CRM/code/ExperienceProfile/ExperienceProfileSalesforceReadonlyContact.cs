using Sitecore.Cintel.ContactService.Model;
using System;
using System.Collections.Generic;
using Sitecore.XConnect;

using System.Data;

namespace Sitecore.HabitatHome.Feature.CRM.ExperienceProfile
{
    [Serializable]
    public class ExperienceProfileSalesforceReadonlyContact : ReadonlyContact
    {
        private string _salesforceContactCreatedDate;
        private string _customSalesforceJourneyStatus;

        public ExperienceProfileSalesforceReadonlyContact(Guid contactId, int classification, List<ContactIdentifier> identifiers, string firstName, string middleName, string surname, string title, string suffix, string nickname, DateTime? birthDate, string gender, string jobTitle, int totalValue, int visitCount, KeyValuePair<string, IAddress> preferredAddress, KeyValuePair<string, IEmailAddress> preferredEmailAddress, KeyValuePair<string, IPhoneNumber> preferredPhoneNumber, IList<KeyValuePair<string, IAddress>> addresses, IList<KeyValuePair<string, IEmailAddress>> emailAddresses, IList<KeyValuePair<string, IPhoneNumber>> phoneNumbers, string salesforceContactCreatedDate, string customSalesforceJourneyStatus) 
            : base(contactId, classification, identifiers, firstName, middleName, surname, title, suffix, nickname, birthDate, gender, jobTitle, totalValue, visitCount, preferredAddress, preferredEmailAddress, preferredPhoneNumber, addresses, emailAddresses, phoneNumbers)
        {
            this._salesforceContactCreatedDate = salesforceContactCreatedDate;
            this._customSalesforceJourneyStatus = customSalesforceJourneyStatus;
        }

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