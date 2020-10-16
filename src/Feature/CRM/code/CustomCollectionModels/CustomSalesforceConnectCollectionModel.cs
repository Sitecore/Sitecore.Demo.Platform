using Sitecore.DataExchange.Tools.SalesforceConnect.Events;
using Sitecore.DataExchange.Tools.SalesforceConnect.Facets;
using Sitecore.XConnect;
using Sitecore.XConnect.Schema;
using Sitecore.XConnect.Collection.Model;
using Sitecore.EmailCampaign.Model.XConnect;

namespace Sitecore.Demo.Platform.Feature.CRM.CustomCollectionModels
{
    public class CustomSalesforceConnectCollectionModel : DataExchange.Tools.SalesforceConnect.Models.SalesforceConnectCollectionModel
    {
        private static XdbModel _model = CustomSalesforceConnectCollectionModel.BuildModel();

        public new static XdbModel Model
        {
            get
            {
                return CustomSalesforceConnectCollectionModel._model;
            }
        }

        private static XdbModel BuildModel()
        {
            XdbModelBuilder xdbModelBuilder = new XdbModelBuilder(typeof(CustomSalesforceConnectCollectionModel).FullName, new XdbModelVersion(1, 0));
            xdbModelBuilder.DefineFacet<Contact, SalesforceContactInformation>("SalesforceContact");
            xdbModelBuilder.DefineFacet<Contact, CustomSalesforceContactInformation>("CustomSalesforceContact");
            xdbModelBuilder.DefineFacet<Contact, SalesforceCampaignMembership>("SalesforceCampaignMembership");
            xdbModelBuilder.DefineFacet<Interaction, SalesforceInteraction>("SalesforceInteraction");
            xdbModelBuilder.DefineEventType<TaskEvent>(false);
            xdbModelBuilder.DefineEventType<EmailEvent>(false);
            xdbModelBuilder.DefineEventType<CallEvent>(false);
            xdbModelBuilder.ReferenceModel(CollectionModel.Model);
            xdbModelBuilder.ReferenceModel(EmailCollectionModel.Model);
            return xdbModelBuilder.BuildModel();
        }
    }
}
