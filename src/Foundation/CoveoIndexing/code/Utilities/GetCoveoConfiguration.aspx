<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="System.Xml" %>
<%@ Import Namespace="Sitecore.Configuration" %>

<script language="c#" runat="server">
	protected void Page_Load(object sender, EventArgs e)
	{
		string apiKeyXPath = "coveo/cloudPlatformConfiguration/apiKey";
		string searchApiKeyXPath = "coveo/cloudPlatformConfiguration/searchApiKey";
		string sitecorePasswordXPath = "coveo/defaultIndexConfiguration/sitecorePassword";

		XmlNode apiKeyNode = Factory.GetConfigNode(apiKeyXPath);
		XmlNode searchApiKeyNode = Factory.GetConfigNode(searchApiKeyXPath);
		XmlNode sitecorePasswordNode = Factory.GetConfigNode(sitecorePasswordXPath);

		if (apiKeyNode == null || searchApiKeyNode == null || sitecorePasswordNode == null)
		{
			Response.Write("{ \"ERROR\": \"Either '" + apiKeyXPath + "', '" + searchApiKeyXPath + "' or '" + sitecorePasswordXPath + "' does not exist in the Sitecore configuration.\" }");
			Response.End();
			return;
		}

		Response.Write("{ \"EncryptedApiKey\": \"" + apiKeyNode.InnerText + "\", \"EncryptedSearchApiKey\": \"" + searchApiKeyNode.InnerText + "\", \"EncryptedSitecorePassword\": \"" + sitecorePasswordNode.InnerText + "\" }");
		Response.End();
	}
</script>