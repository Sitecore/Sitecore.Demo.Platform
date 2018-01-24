using Microsoft.Extensions.DependencyInjection;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.SubmitActions;
using Sitecore.Feature.Forms.SubmitActions.Models;
using Sitecore.Foundation.Accounts.Models;
using Sitecore.Foundation.Accounts.Services;
using Sitecore.Foundation.SitecoreExtensions.Services;
using Sitecore.SecurityModel;
using System;
using System.Linq;

namespace Sitecore.Feature.Forms.SubmitActions
{
    public class IdentifyContactAction : AnalyticsActionBase<IdentifyContactActionData>
    {
        private readonly ITrackerService _trackerService;
        private readonly IContactFacetService _contactFacetService;   

        public IdentifyContactAction(ISubmitActionData submitActionData) : base(submitActionData)
        {
            _trackerService = ServiceLocator.ServiceProvider.GetService<ITrackerService>();
            _contactFacetService = ServiceLocator.ServiceProvider.GetService<IContactFacetService>();
        }

        protected override bool Execute(IdentifyContactActionData data, FormSubmitContext formSubmitContext)
        {
            Assert.ArgumentNotNull(formSubmitContext, "formSubmitContext");
            if (data == null || !(data.ReferenceId != Guid.Empty))
            {
                // submit action was not configured properly     
                Log.Error(string.Format("IdentifyContactAction failed: Submit action settings were not configured properly for form {0}.", formSubmitContext.FormId), this);
                return false;
            }
                                                                                         
            //todo: use the data.ReferenceId to retrieve mapping of fields to contact facets
            var formSettingsFolder = Context.Database.GetItem(new ID(data.ReferenceId));

            ContactFacetData contactFacetData = new ContactFacetData();

            foreach(var field in formSubmitContext.Fields)
            {
                if (field != null)
                {
                    var mapSettingsItem = formSettingsFolder.Children.FirstOrDefault(x =>
                        x[Templates.FacetActionMapping.Fields.FacetValue].ToLower() == field.Name.ToLower());
                    if (mapSettingsItem != null)
                    {
                        Item facetItem;

                        using (new SecurityDisabler())
                        {
                            facetItem = Sitecore.Configuration.Factory.GetDatabase("core").GetItem(mapSettingsItem[Templates.FacetActionMapping.Fields.FacetKey]);
                        }

                        if (facetItem != null)
                        {
                            contactFacetData[facetItem.Name] = field.GetValue();
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(contactFacetData.EmailAddress))
            {
                _trackerService.IdentifyContact(Context.Site.Domain.Name, contactFacetData.EmailAddress);
            }

            _contactFacetService.UpdateContactFacets(contactFacetData);

            return true;
        }
    }
}