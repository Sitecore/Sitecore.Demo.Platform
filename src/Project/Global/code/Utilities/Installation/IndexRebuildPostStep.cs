using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.Install.Framework;

namespace  Sitecore.HabitatHome.Global.Website.Utilities.Installation
{
    public class IndexRebuildPostStep : IPostStep
    {
        private readonly string[] _indexNamesToRebuild =
            new[] {"sitecore_core_index", "sitecore_master_index", "sitecore_web_index"};

        public void Run(ITaskOutput output, NameValueCollection metaData)
        {
            //if (_indexNamesToRebuild.Any())
            //{
            //    foreach (string indexName in _indexNamesToRebuild)
            //    {
            //        //RebuildIndex(indexName);
            //    }
            //}
        }

        private void RebuildIndex(string indexName)
        {
            try
            {
                Diagnostics.Log.Info(string.Format("Rebuilding index {0}", indexName), this);

                ISearchIndex index = ContentSearchManager.GetIndex(indexName);

                if (index != null)
                {
                    IndexCustodian.FullRebuild(index);

                    //while (IndexCustodian.IsRebuilding(index))
                    //{
                    //    Thread.Sleep(1000);
                    //}
                    
                }
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error("Error Rebuilding Index", ex, this);
            }
        }
    }
}