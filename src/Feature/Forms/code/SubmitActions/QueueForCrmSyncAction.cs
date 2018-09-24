using Microsoft.Extensions.DependencyInjection;
using Sitecore.Analytics;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.SubmitActions;
using Sitecore.HabitatHome.Feature.CRM.SubmitActions.Models;
using Sitecore.HabitatHome.Foundation.Accounts.Models;
using Sitecore.HabitatHome.Foundation.Accounts.Services;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Extensions;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Services;
using Sitecore.SecurityModel;
using Sitecore.XConnect.Collection.Model;
using System;
using System.Linq;

namespace Sitecore.HabitatHome.Feature.Forms.SubmitActions
{
    public class QueueForCrmSyncAction : AnalyticsActionBase<QueueForCrmSyncActionData>
    {
        private readonly ITrackerService _trackerService;
        private readonly IContactFacetService _contactFacetService;   

        public QueueForCrmSyncAction(ISubmitActionData submitActionData) : base(submitActionData)
        {
            _trackerService = ServiceLocator.ServiceProvider.GetService<ITrackerService>();
            _contactFacetService = ServiceLocator.ServiceProvider.GetService<IContactFacetService>();
        }

        protected override bool Execute(QueueForCrmSyncActionData data, FormSubmitContext formSubmitContext)
        {
            Assert.ArgumentNotNull(formSubmitContext, "formSubmitContext");
            if (data == null || !(data.ReferenceId != Guid.Empty))
            {
                // submit action was not configured properly     
                Log.Error(string.Format("IdentifyContactAction failed: Submit action settings were not configured properly for form {0}.", formSubmitContext.FormId), this);
                return false;
            }

            //todo: use the data.ReferenceId to retrieve mapping of fields to contact facets
            var item = Context.Database.GetItem(new ID(data.ReferenceId));
            if (item == null || !item.IsDerived(Templates.ContactIdentificationActionSettings.ID))
            {
                // submit action was not configured properly    
                Log.Error(string.Format("IdentifyContactAction failed: Submit action settings for form {0} point to an invalid item.", formSubmitContext.FormId), this);
                return false;
            }

            ContactFacetData contactFacetData = new ContactFacetData();

            foreach (var field in formSubmitContext.Fields)
            {
                if (field != null)
                {
                    var mapSettingsItem = item.Children.FirstOrDefault(x =>
                        x[Templates.ContactIdentificationActionMapping.Fields.FacetValue].ToLower() == field.Name.ToLower());
                    if (mapSettingsItem != null)
                    {
                        Item facetItem;

                        using (new SecurityDisabler())
                        {
                            facetItem = Sitecore.Configuration.Factory.GetDatabase("core").GetItem(mapSettingsItem[Templates.ContactIdentificationActionMapping.Fields.FacetKey]);
                        }

                        if (facetItem != null)
                        {
                            contactFacetData[facetItem.Name] = field.GetValue();
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(contactFacetData.PhoneNumber))
            {
                contactFacetData.PhoneKey = "Preferred";
            }

            if (!string.IsNullOrEmpty(contactFacetData.EmailAddress))
            {
                contactFacetData.EmailKey = "Preferred";
            }

            _contactFacetService.UpdateContactFacets(contactFacetData);

            return true;
        }
    }
}