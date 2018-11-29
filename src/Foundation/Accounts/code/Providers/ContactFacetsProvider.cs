using Sitecore.XConnect.Collection.Model.Cache;

namespace Sitecore.HabitatHome.Foundation.Accounts.Providers
{
    using Sitecore.Analytics;
    using Sitecore.Analytics.Model;
    using Sitecore.Analytics.Tracking;
    using Sitecore.Analytics.XConnect.Facets;
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.HabitatHome.Foundation.Accounts.Models.Facets;
    using Sitecore.HabitatHome.Foundation.DependencyInjection;
    using Sitecore.XConnect;
    using Sitecore.XConnect.Collection.Model;

    [Service(typeof(IContactFacetsProvider), Lifetime = Lifetime.Transient)]
    public class ContactFacetsProvider : IContactFacetsProvider
    {
        private readonly ContactManager _contactManager;

        public ContactFacetsProvider()
        {
            _contactManager = Factory.CreateObject("tracking/contactManager", true) as ContactManager;
        }
                                                                                                                                                                        
        public PersonalInformation PersonalInfo => GetFacet<PersonalInformation>(PersonalInformation.DefaultFacetKey);

        public AddressList Addresses => GetFacet<AddressList>(AddressList.DefaultFacetKey);

        public EmailAddressList Emails => GetFacet<EmailAddressList>(EmailAddressList.DefaultFacetKey);

        public ConsentInformation CommunicationProfile => GetFacet<ConsentInformation>(ConsentInformation.DefaultFacetKey);

        public PhoneNumberList PhoneNumbers => GetFacet<PhoneNumberList>(PhoneNumberList.DefaultFacetKey);

        public SportType SportType => GetFacet<SportType>(SportType.DefaultKey);

        public SportName SportName => GetFacet<SportName>(SportName.DefaultKey);

        public Analytics.Tracking.Contact Contact
        {
            get
            {
                if (!Tracker.IsActive)
                {
                    return null;
                }

                var contact = Tracker.Current.Contact;
                return contact ?? _contactManager?.CreateContact(ID.NewID);
            }
        }

        public Avatar Picture => GetFacet<Avatar>(Avatar.DefaultFacetKey);

        public bool IsKnown => Tracker.Current?.Contact?.IdentificationLevel == ContactIdentificationLevel.Known;

        public InteractionsCache InteractionsCache => GetFacet<InteractionsCache>("InteractionsCache");

        protected T GetFacet<T>(string facetName) where T : Facet
        {
            var xConnectFacet = Tracker.Current.Contact.GetFacet<IXConnectFacets>("XConnectFacets");
            var allFacets = xConnectFacet.Facets;
            if (allFacets == null)
                return null;
            if (!allFacets.ContainsKey(facetName))
                return null;

            return (T)allFacets?[facetName];
        }
    }
}