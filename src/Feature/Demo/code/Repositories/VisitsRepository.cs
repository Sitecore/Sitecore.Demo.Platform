namespace Sitecore.Feature.Demo.Repositories
{
    using Sitecore.Analytics;
    using Sitecore.Analytics.Tracking;
    using Sitecore.Feature.Demo.Models;
    using Sitecore.Foundation.DependencyInjection;
    using Sitecore.XA.Foundation.Mvc.Repositories.Base;
    using System.Collections.Generic;
    using System.Linq;

    [Service(typeof(IVisitsRepository))]
    public class VisitsRepository : ModelRepository, IVisitsRepository
    {
        //private readonly IContactFacetsProvider contactFacetsProvider;
        private readonly EngagementPlanStateRepository engagementPlanStateRepository;
        private readonly PageViewRepository pageViewRepository;

        public VisitsRepository(EngagementPlanStateRepository engagementPlanStateRepository, PageViewRepository pageViewRepository)
        {
            this.engagementPlanStateRepository = engagementPlanStateRepository;
            this.pageViewRepository = pageViewRepository;
        }

        public override IRenderingModelBase GetModel()
        {
            Visits model = new Visits();
            FillBaseProperties(model);

            var allPageViews = this.GetAllPageViews().ToArray();
            model.EngagementValue = GetEngagementValue();
            model.PageViews = allPageViews.Take(10);
            model.TotalPageViews = allPageViews.Length;
            model.TotalVisits = GetTotalVisits();
            model.EngagementPlanStates = engagementPlanStateRepository.GetCurrent().ToArray();

            return model;
        }

        private int GetEngagementValue()
        {
            return 0; //this.contactFacetsProvider.Contact?.System.Value ?? 0;
        }

        private int GetTotalVisits()
        {
            return 1;  //this.contactFacetsProvider.Contact?.System?.VisitCount ?? 1;
        }

        private IEnumerable<PageView> GetAllPageViews()
        {
            return Tracker.Current.Interaction.GetPages().Cast<ICurrentPageContext>().Where(x => !x.IsCancelled).Select(pc => (PageView)pageViewRepository.Get(pc)).Reverse();
        }
    }
}