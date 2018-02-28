<%@ page language="C#" autoeventwireup="true" inherits="Sitecore.EmailCampaign.Cm.UI.sitecore.admin.MessageStatistics" %>

<%@ import namespace="Sitecore.Configuration" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Message Statistics</title>

    <script runat="server" language="c#">

        private string GetApiKey()
        {
            return Settings.GetSetting("MessageStatistics.ApiKey");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string apiKey = Request.QueryString["apiKey"];
            if (!string.Equals(apiKey, GetApiKey()))
            {
                Response.Write("ApiKey is wrong dude");
                Response.End();
            }

            try
            {
                Upgrade_Click(sender, e);
                Response.Write("OK");
            }
            catch (Exception ex)
            {
                Response.Write("ERROR: " + ex.Message);
            }

            Response.End();
        }
    </script>
</head>
<body>
</body>
</html>
