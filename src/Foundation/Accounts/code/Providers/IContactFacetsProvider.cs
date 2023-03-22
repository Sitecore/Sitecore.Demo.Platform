using Sitecore.Analytics.Tracking;
using Sitecore.XConnect.Collection.Model;

namespace Sitecore.Demo.Platform.Foundation.Accounts.Providers
{
    public interface IContactFacetsProvider
    {
        Contact Contact { get; }
        XConnect.Collection.Model.Cache.InteractionsCache InteractionsCache { get; }
        PersonalInformation PersonalInfo { get; }
        AddressList Addresses { get; }
        EmailAddressList Emails { get; }
        ConsentInformation CommunicationProfile { get; }
        PhoneNumberList PhoneNumbers { get; }
        Avatar Picture { get; }
        bool IsKnown { get; }
    }
}
