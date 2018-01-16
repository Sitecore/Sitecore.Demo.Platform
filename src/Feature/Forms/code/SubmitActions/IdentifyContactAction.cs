using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.SubmitActions;
using Sitecore.Feature.Forms.SubmitActions.Models;
using Sitecore.Foundation.Accounts.Models;
using Sitecore.Foundation.Accounts.Services;
using Sitecore.Foundation.SitecoreExtensions.Services;
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

            ContactFacetData contactFacetData = new ContactFacetData();

            foreach(var field in formSubmitContext.Fields)
            {
                if (field != null)
                {
                    ProcessField(contactFacetData, field);
                }
            }

            _contactFacetService.UpdateContactFacets(contactFacetData);

            return true;
        }

        private void ProcessField(ContactFacetData data, IViewModel field)
        {
            //todo: use mapping data instead of hard coded values
            switch (field.Name)
            {
                case "Email":
                case "Email Address":
                    string emailAddress = field.GetValue();
                    if (!string.IsNullOrEmpty(emailAddress))
                    {                                                                                               
                        _trackerService.IdentifyContact(Context.Site.Domain.Name, emailAddress);
                        data.EmailAddress = emailAddress;
                    }
                    break;
                case "FirstName":
                case "First Name":
                    string firstName = field.GetValue();
                    if (!string.IsNullOrEmpty(firstName))
                    {
                        data.FirstName = firstName;
                    }
                    break;
                case "LastName":
                case "Last Name":
                    string lastName = field.GetValue();
                    if (!string.IsNullOrEmpty(lastName))
                    {
                        data.LastName = lastName;
                    }
                    break;
                case "FullName":
                case "Full Name":
                    string fullName = field.GetValue();
                    if (!string.IsNullOrEmpty(fullName))
                    {
                        data.FirstName = fullName.Split(' ').FirstOrDefault();
                        data.LastName = fullName.Split(' ').LastOrDefault();
                    }
                    break;
                default: break;
            }
        }
    }
}