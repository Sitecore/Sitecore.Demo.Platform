using Sitecore.Analytics;
using Sitecore.Analytics.Model;
using Sitecore.Analytics.XConnect.Facets;
using Sitecore.Demo.Platform.Foundation.Accounts.Models.Facets;
using Sitecore.Demo.Platform.Foundation.DependencyInjection;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Collection.Model.Cache;

namespace Sitecore.Demo.Platform.Foundation.Accounts.Providers
{
    [Service(typeof(IContactFacetsProvider), Lifetime = Lifetime.Transient)]
    public class ContactFacetsProvider : IContactFacetsProvider
    {
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

                return Tracker.Current.Contact;
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