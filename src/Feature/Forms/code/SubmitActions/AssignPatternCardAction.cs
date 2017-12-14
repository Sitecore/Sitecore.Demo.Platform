using Sitecore.ExperienceForms.Processing.Actions;
using Sitecore.ExperienceForms.Processing.Actions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.Models;
using Sitecore.Diagnostics;
using Sitecore.Data;

namespace Sitecore.Feature.Forms.SubmitActions
{
    public class AssignPatternCardAction : SubmitActionBase<RedirectActionData>
    {
        protected AssignPatternCardAction(ISubmitActionData submitActionData) : base(submitActionData)
        {
        }

        protected override bool Execute(RedirectActionData data, FormSubmitContext formSubmitContext)
        {
            Assert.ArgumentNotNull(formSubmitContext, "formSubmitContext");
            if (data == null || !(data.ReferenceId != Guid.Empty))
                return false;
            var item = Sitecore.Context.Database.GetItem(new ID(data.ReferenceId));
            if (item == null)
                return false;

            var email = string.Empty;
            var field = formSubmitContext.Fields.FirstOrDefault(f => f.Name.Equals("Email"));
            if (field != null)
            {
                var property = field.GetType().GetProperty("Value");
                var postedEmail = property.GetValue(field);          
            }
                                                 
            return true;
        }
    }
}