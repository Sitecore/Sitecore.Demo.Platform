using Sitecore.Analytics;
using Sitecore.Diagnostics;
using Sitecore.EmailCampaign.Cd.Pipelines.RedirectUrl;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.Framework.Conditions;
using Sitecore.Modules.EmailCampaign.Core.Gateways;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using System;

namespace Sitecore.Demo.Platform.Feature.Email.Processors
{
    public class IdentifyContactAndLoadFacets
    {
        public readonly ILogger _logger;
        public readonly AnalyticsGateway _analyticsGateway;

        public IdentifyContactAndLoadFacets(AnalyticsGateway analyticsGateway, ILogger logger)
        {
            Assert.ArgumentNotNull((object)analyticsGateway, nameof(analyticsGateway));
            Assert.ArgumentNotNull((object)logger, nameof(logger));
            _analyticsGateway = analyticsGateway;
            _logger = logger;
        }

          public void Process(RedirectUrlPipelineArgs args)
          {
               Assert.ArgumentNotNull((object)args, nameof(args));
               if (!args.RegisterEvents)
                    return;
               try
               {
                    IdentifyContact(args.EventData.ContactIdentifier);
               }
               catch (Exception ex)
               {
                    _logger.LogError("Failed to identify contact", ex);
               }
          }

          public void IdentifyContact(ContactIdentifier contactIdentifier)
          {
               Condition.Requires<ContactIdentifier>(contactIdentifier, nameof(contactIdentifier)).IsNotNull<ContactIdentifier>();
               Condition.Requires<ITracker>(Tracker.Current, "Current").IsNotNull<ITracker>();
               Tracker.Current.Session.IdentifyAs(contactIdentifier.Source, contactIdentifier.Identifier);
          }
    }
}