using Sitecore.Cintel.Endpoint.Plumbing;
using System;
using System.Web;
using System.Web.Http;
using Sitecore.Cintel.Configuration;
using Sitecore.Cintel.ContactService;
using Sitecore.Cintel.ContactService.Model;
using Sitecore.Cintel.Endpoint.Transfomers;
using Sitecore.Web.Http.Filters;
using System.Net;
using System.Net.Http;

namespace Sitecore.HabitatHome.Feature.CRM.ExperienceProfile.CustomApiController
{
    [NegotiateLanguageFilter]
    [AuthorizedReportingUserFilter]
    public class ExperienceProfileSalesforceContactController : ApiController
    {
        [HttpGet]
        [ValidateHttpAntiForgeryToken]
        public object Get(Guid contactId)
        {
            try
            {
                return ApplyContactDetailsTransformer(ExperienceProfileSalesforceCustomerIntelligenceManager.ContactService.Get(contactId));
            }
            catch (ContactNotFoundException ex)
            {
                return (object)this.Request.CreateResponse<string>(HttpStatusCode.NotFound, ex.Message);
            }
        }

        private static object ApplyContactDetailsTransformer(IContact contact)
        {
            Cintel.Commons.ResultSet<IContact> resultSet1 = new Cintel.Commons.ResultSet<IContact>(1, 1);
            resultSet1.Data.Dataset.Add(nameof(contact), contact);
            string header1 = HttpContext.Current.Request.Headers[WellknownIdentifiers.TransfomerClientNameHeader];
            string header2 = HttpContext.Current.Request.Headers[WellknownIdentifiers.TransformerKeyHeader];
            if (string.IsNullOrEmpty(header1) || string.IsNullOrEmpty(header2))
                return (object)contact;
            Cintel.Commons.ResultSet<IContact> resultSet2 = ResultTransformManager.GetContactDetailsTransformer(header1, header2).Transform(resultSet1) as Cintel.Commons.ResultSet<IContact>;
            if (resultSet2 == null)
                return (object)contact;
            return (object)resultSet2.Data.Dataset[nameof(contact)];
        }
    }
}