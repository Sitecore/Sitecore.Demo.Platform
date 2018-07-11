using Sitecore.ExperienceEditor.Speak.Server.Responses;        
using Sitecore.SecurityModel;    

namespace Sitecore.HabitatHome.Feature.Accounts.ExperienceEditor
{       
    public class LargeDropDownButtonChildItems : Sitecore.ExperienceEditor.Speak.Ribbon.Requests.LargeDropDownButton.LargeDropDownButtonChildItems
    {
        public override PipelineProcessorResponseValue ProcessRequest()
        {                
            // with fed auth enabled, the context user in Preview does not have access to dropdown - overriding to allow
            using (new SecurityDisabler())
            {
                return base.ProcessRequest();
            }
        }            
    }        
}