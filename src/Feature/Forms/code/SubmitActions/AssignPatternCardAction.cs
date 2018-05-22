using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.Models;
using Sitecore.Diagnostics;
using Sitecore.Data;
using Sitecore.Data.Items;
using System.Xml;               
using Sitecore.ExperienceForms.SubmitActions;
using Sitecore.Analytics;
using Sitecore.Analytics.Tracking;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.HabitatHome.Feature.Forms.SubmitActions
{
    public class AssignPatternCardAction : AnalyticsActionBase<AssignPatternActionData>
    {
        public AssignPatternCardAction(ISubmitActionData submitActionData) : base(submitActionData)
        {
        }

        protected override bool Execute(AssignPatternActionData data, FormSubmitContext formSubmitContext)
        {
            Assert.ArgumentNotNull(formSubmitContext, "formSubmitContext");
            if (data == null || !(data.ReferenceId != Guid.Empty))
            {
                // submit action was not configured properly     
                Log.Error(string.Format("AssignPatternCardAction failed: Submit action settings were not configured properly for form {0}.", formSubmitContext.FormId), this);
                return false;
            }

            Item item = Context.Database.GetItem(new ID(data.ReferenceId));
            if (item == null || !item.IsDerived(Templates.PatternCardActionSettings.ID))
            {
                // submit action was not configured properly    
                Log.Error(string.Format("AssignPatternCardAction failed: Submit action settings for form {0} point to an invalid item.", formSubmitContext.FormId), this);
                return false;
            }

            string questionId = item[Templates.PatternCardActionSettings.Fields.FormQuestion];
            if (questionId == null)
            {
                // settings item was not configured properly
                Log.Error(string.Format("AssignPatternCardAction failed: Submit action settings were not configured properly. Form Question on settings item {0} is not assigned or does not exist in this context.", data.ReferenceId), this);
                return false;
            }

            questionId = questionId.Replace("{", string.Empty).Replace("}", string.Empty).ToLower();
            IViewModel fieldModel = formSubmitContext.Fields.FirstOrDefault(f => f.ItemId == questionId);
            if (fieldModel == null)
            {
                // no submitted field matched the configured form question          
                Log.Error(string.Format("AssignPatternCardAction failed: Configured question ID {0} does not exist form {1}", questionId, formSubmitContext.FormId), this);
                return false;
            }

            string answer = string.Empty;
            try
            {
                answer = fieldModel.GetValue();
            }
            catch (Exception ex)
            {
                // could not get value of submitted field
                Log.Error("AssignPatternCardAction failed: could not parse value of submitted field.", ex, this);
                return false;
            }

            var mappings = item.GetChildren();
            var mapping = mappings.FirstOrDefault(m => m[Templates.PatternCardActionMapping.Fields.MappedValue].ToLower().Trim() == answer.ToLower().Trim());
            if (mapping == null)
            {
                // could not find a mapping setting item for the submitted value
                // log warning and continue execution without failure
                Log.Warn(string.Format("AssignPatternCardAction failed: No mapping found for answer {0} of question {1}", answer, questionId), this);
                return true;
            }

            string patternCardId = mapping["Pattern Card"];
            Item matchedPatternCard = Context.Database.GetItem(patternCardId);

            try
            {
                if (Tracker.Current == null)
                {
                    Tracker.StartTracking();
                }
               
                Profile profile = Tracker.Current.Interaction.Profiles[matchedPatternCard.Parent.Parent.Name];

                Data.Fields.XmlField xmlData = matchedPatternCard.Fields["Pattern"];
                XmlDocument xmlDoc = xmlData.Xml;

                XmlNodeList parentNode = xmlDoc.GetElementsByTagName("key");
                Dictionary<string, double> scores = new Dictionary<string, double>();

                foreach (XmlNode childrenNode in parentNode)
                {
                    if (childrenNode.Attributes["value"].Value != "0")
                    {
                        double keyValue;
                        double.TryParse(childrenNode.Attributes["value"].Value, out keyValue);
                        scores.Add(childrenNode.Attributes["name"].Value, keyValue * 3);
                    }
                }

                profile.Score(scores);
                profile.UpdatePattern();
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("AssignPatternCardAction failed: Could not update pattern for card {0}", patternCardId), ex, this);
                return false;
            }                 

            return true;
        }
    }
}