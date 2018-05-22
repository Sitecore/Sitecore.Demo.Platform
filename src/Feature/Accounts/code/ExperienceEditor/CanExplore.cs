using Sitecore.Abstractions;
using Sitecore.ExperienceEditor.Speak.Server.Responses;
using Sitecore.SecurityModel;

namespace Sitecore.HabitatHome.Feature.Accounts.ExperienceEditor
{
    public class CanExplore : Sitecore.ExperienceExplorer.Web.Requests.CanExplore
    {
        public CanExplore(ExperienceExplorer.Core.Security.IExplorerUserContext userContext, BaseSettings settings) : base(userContext, settings)
        {
        }

        public override PipelineProcessorResponseValue ProcessRequest()
        {
            // with fed auth enabled, the context user in Preview never has CanExplore permissions - overriding to allow
            using (new SecurityDisabler())
            {
                return base.ProcessRequest();
            }
        }
    }
}