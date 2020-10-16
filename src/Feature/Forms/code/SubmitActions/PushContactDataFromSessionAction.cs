using System;
using System.Web;
using Sitecore.Analytics;
using Sitecore.Diagnostics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.Processing.Actions;

namespace Sitecore.Demo.Platform.Feature.Forms.SubmitActions
{
    public class PushContactDataFromSessionAction : SubmitActionBase<string>
    {
        public PushContactDataFromSessionAction(ISubmitActionData submitActionData) : base(submitActionData)
        {
        }

        protected override bool TryParse(string value, out string target)
        {
            target = string.Empty;
            return true;
        }

        protected override bool Execute(string data, FormSubmitContext formSubmitContext)
        {
            try
            {
                if (Tracker.Current == null && Tracker.Enabled)
                {
                    Tracker.StartTracking();
                }

                HttpContext.Current.Session?.Abandon();
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Error occured while updating xDB contact data with custom submit action"), ex, this);
                return false;
            }

            return true;
        }
    }
}