using System;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Maintenance;

namespace Sitecore.HabitatHome.Global.Website.Utilities
{
    public partial class IndexRebuild : System.Web.UI.Page
    {
        private string _indexName;
        private readonly string _parameter = "index";

        protected void Page_Load(object sender, EventArgs e)
        {
            IndexRebuildError.Visible = false;
            IndexNotFound.Visible = false;
            IndexRebuilding.Visible = false;

            if (!string.IsNullOrWhiteSpace(Request.QueryString[_parameter]))
            {
                _indexName = Request.QueryString[_parameter];
                RebuildIndex();
            }
        }

        protected void RebuildIndexButton_OnClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                _indexName = LoginTextBox.Text;
                RebuildIndex();
            }
        }

        private void RebuildIndex()
        {
            try
            {
                var index = ContentSearchManager.GetIndex(_indexName);
                if (index != null)
                {
                    IndexRebuilding.Visible = true;
                    IndexCustodian.FullRebuild(index, true);
                }
                else
                {
                    IndexNotFound.Visible = true;
                }
            }
            catch (Exception ex)
            {
                IndexRebuildError.Text = ex.Message;
                IndexRebuildError.Visible = true;
            }
        }
    }
}