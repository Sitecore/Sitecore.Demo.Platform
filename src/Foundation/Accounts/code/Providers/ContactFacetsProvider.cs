using Sitecore.Analytics;
using Sitecore.Analytics.Model;
using Sitecore.Analytics.Tracking;
using Sitecore.Analytics.XConnect.Facets;
using Sitecore.Demo.Platform.Foundation.DependencyInjection;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Collection.Model.Cache;

namespace Sitecore.Demo.Platform.Foundation.Accounts.Providers
{
    [Service(typeof(IContactFacetsProvider), Lifetime = Lifetime.Transient)]
    public class ContactFacetsProvider : IContactFacetsProvider
    {
        private readonly ContactManager contactManager;

        public ContactFacetsProvider()
        {
            contactManager = Configuration.Factory.CreateObject("tracking/contactManager", true) as ContactManager;
        }

        public PersonalInformation PersonalInfo => GetFacet<PersonalInformation>(PersonalInformation.DefaultFacetKey);

        public AddressList Addresses => GetFacet<AddressList>(AddressList.DefaultFacetKey);

        public EmailAddressList Emails => GetFacet<EmailAddressList>(EmailAddressList.DefaultFacetKey);

        public ConsentInformation CommunicationProfile => GetFacet<ConsentInformation>(ConsentInformation.DefaultFacetKey);

        public PhoneNumberList PhoneNumbers => GetFacet<PhoneNumberList>(PhoneNumberList.DefaultFacetKey);

        public Analytics.Tracking.Contact Contact
        {
            get
            {
                if (!Tracker.IsActive)
                {
                    return null;
                }

                return Tracker.Current.Contact;
            }
        }

        public Avatar Picture => GetFacet<Avatar>(Avatar.DefaultFacetKey);

        public bool IsKnown => Tracker.Current?.Contact?.IdentificationLevel == ContactIdentificationLevel.Known;

        public InteractionsCache InteractionsCache => GetFacet<InteractionsCache>("InteractionsCache");

        protected T GetFacet<T>(string facetName) where T : Facet
        {
            if (Tracker.Current?.Contact == null)
            {
                return null;
            }

            var contact = contactManager.LoadContact(Tracker.Current.Contact.ContactId);
            if (contact == null)
            {
                return null;
            }

            var xConnectFacet = contact.GetFacet<IXConnectFacets>("XConnectFacets");
            var allFacets = xConnectFacet.Facets;
            if (allFacets == null)
                return null;
            if (!allFacets.ContainsKey(facetName))
                return null;

            return (T)allFacets?[facetName];
        }
    }
}
