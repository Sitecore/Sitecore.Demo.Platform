using Coveo.Framework.CNL;
using Coveo.Framework.Items;
using Coveo.Framework.Log;
using Coveo.SearchProvider.InboundFilters;
using Coveo.SearchProvider.Pipelines;
using System.Reflection;

namespace Sitecore.Demo.Platform.Foundation.CoveoIndexing.Processors
{
	public class ExcludeItemsCoveoInboundFilter : AbstractCoveoInboundFilterProcessor
	{
		private static readonly ILogger s_Logger = CoveoLogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private string m_UppercaseItemIds;

		/// <summary>
		/// IDs of items to exclude.
		/// </summary>
		/// <remarks>If multiple item IDs are specified, they must be separated by a semicolon.</remarks>
		public string ItemIds
		{
			set
			{
				m_UppercaseItemIds = value.ToUpper();
			}
		}

		public override void Process(CoveoInboundFilterPipelineArgs p_Args)
		{
			s_Logger.TraceEntering("Process(CoveoInboundFilterPipelineArgs)");
			Precondition.NotNull(p_Args, () => () => p_Args);

			s_Logger.Trace("EXCLUDE ITEM: All Items Ids to be excluded: " + m_UppercaseItemIds);
			s_Logger.Trace("EXCLUDE ITEM:" + p_Args.IndexableToIndex + " " + p_Args.IndexableToIndex.Item + " " +
					ShouldExecute(p_Args) + " " + ShouldExcludeItem(p_Args.IndexableToIndex.Item));

			if (p_Args.IndexableToIndex != null &&
					p_Args.IndexableToIndex.Item != null &&
					ShouldExecute(p_Args) &&
					ShouldExcludeItem(p_Args.IndexableToIndex.Item))
			{
				s_Logger.Debug("The item \"{0}\" will not be indexed because it is explicitely excluded.", p_Args.IndexableToIndex.Item.ID.ToString());
				p_Args.IsExcluded = true;
			}

			s_Logger.TraceExiting("Process(CoveoInboundFilterPipelineArgs)");
		}

		private bool ShouldExcludeItem(IItem p_Item)
		{
			s_Logger.Trace("EXCLUDE ITEM: All Items Ids to be excluded: " + m_UppercaseItemIds);
			return m_UppercaseItemIds.Contains(p_Item.ID.ToString().ToUpper());
		}
	}
}
