using Sitecore.HabitatHome.Foundation.Accounts.Models.Facets;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Schema;

namespace Sitecore.HabitatHome.Foundation.Accounts.Models
{
    public class AccountCollectionModel
    {
        public static XdbModel Instance { get; } = CreateModel();

        public static XdbModel CreateModel()
        {
            var builder = new XdbModelBuilder(typeof(AccountCollectionModel).FullName, new XdbModelVersion(1, 0));

            builder.ReferenceModel(CollectionModel.Model);
            builder.DefineFacet<Contact, SportName>(SportName.DefaultKey);
            builder.DefineFacet<Contact, SportType>(SportType.DefaultKey);

            return builder.BuildModel();
        }

        public static string CreateDeploymentJson(out string modelName)
        {
            XdbModel model = CreateModel();
            modelName = model.ToString();
            var json = Sitecore.XConnect.Serialization.XdbModelWriter.Serialize(model);
            return json;
        }
    }
}