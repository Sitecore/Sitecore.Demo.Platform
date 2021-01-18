using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Newtonsoft.Json;
using Sitecore.Configuration;

namespace Sitecore.Demo.Platform.Foundation.CoveoIndexing.Utilities
{
	class CoveoIsActivatedStatus
	{
		public bool IsActivated { get; set; }
	}

	class CoveoConfiguration
	{
		public string EncryptedApiKey { get; set; }
		public string EncryptedSearchApiKey { get; set; }
		public string OrganizationId { get; set; }
		public string FarmName { get; set; }
		public string SitecoreUsername { get; set; }
		public string EncryptedSitecorePassword { get; set; }
	}

	public partial class PostCmCoveoActivation : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsCoveoActivatedOnCm())
			{
				SendResponse("ERROR: Coveo is not activated on the CM. Please manually configure and activate Coveo first.");
			}

			CoveoConfiguration cmConfiguration = GetCoveoCmConfiguration();
			if (cmConfiguration == null)
			{
				SendResponse("ERROR: Failed to get the Coveo CM configuration.");
			}

			bool configurationSucceded = ConfigureAndActivateCd(cmConfiguration).GetAwaiter().GetResult();
			if (!configurationSucceded)
			{
				SendResponse("ERROR: Failed to configure and activate Coveo on the CD.");
			}

			configurationSucceded = ActivateCdCustomization().GetAwaiter().GetResult();
			if (!configurationSucceded)
			{
				SendResponse("ERROR: Failed to activate Coveo customizations on the CD.");
			}

			ActivateCmCustomization();

			SendResponse("SUCCESS: Coveo configured and activated on both the CD and CM.");
		}

		private void SendResponse(string message)
		{
			Response.Write(message);
			Response.End();
		}

		private bool IsCoveoActivatedOnCm()
		{
			return File.Exists("C:\\inetpub\\wwwroot\\App_Config\\Include\\Coveo\\Coveo.SearchProvider.Custom.config");
		}

		private CoveoConfiguration GetCoveoCmConfiguration()
		{
			string cloudPlatformConfigurationXPath = "coveo/cloudPlatformConfiguration";
			string apiKeyXPath = $"{cloudPlatformConfigurationXPath}/apiKey";
			string searchApiKeyXPath = $"{cloudPlatformConfigurationXPath}/searchApiKey";
			string organizationIdXPath = $"{cloudPlatformConfigurationXPath}/organizationId";

			string defaultIndexConfigurationXPath = "coveo/defaultIndexConfiguration";
			string farmNameXPath = $"{defaultIndexConfigurationXPath}/farmName";
			string sitecoreUsernameXPath = $"{defaultIndexConfigurationXPath}/sitecoreUsername";
			string sitecorePasswordXPath = $"{defaultIndexConfigurationXPath}/sitecorePassword";

			XmlNode apiKeyNode = Factory.GetConfigNode(apiKeyXPath);
			XmlNode searchApiKeyNode = Factory.GetConfigNode(searchApiKeyXPath);
			XmlNode organizationIdNode = Factory.GetConfigNode(organizationIdXPath);

			XmlNode farmNameNode = Factory.GetConfigNode(farmNameXPath);
			XmlNode sitecoreUsernameNode = Factory.GetConfigNode(sitecoreUsernameXPath);
			XmlNode sitecorePasswordNode = Factory.GetConfigNode(sitecorePasswordXPath);

			if (apiKeyNode == null || searchApiKeyNode == null || organizationIdNode == null || farmNameNode == null || sitecoreUsernameNode == null || sitecorePasswordNode == null)
			{
				return null;
			}

			return new CoveoConfiguration() {
				EncryptedApiKey = apiKeyNode.InnerText,
				EncryptedSearchApiKey = searchApiKeyNode.InnerText,
				OrganizationId = organizationIdNode.InnerText,
				FarmName = farmNameNode.InnerText,
				SitecoreUsername = sitecoreUsernameNode.InnerText,
				EncryptedSitecorePassword = sitecorePasswordNode.InnerText
			};
		}

		private async Task<bool> ConfigureAndActivateCd(CoveoConfiguration cmConfiguration)
		{
			var hostCD = Environment.GetEnvironmentVariable("HOST_CD");

			var organizationId = HttpUtility.UrlEncode(cmConfiguration.OrganizationId);
			var apiKey = HttpUtility.UrlEncode(cmConfiguration.EncryptedApiKey);
			var searchApiKey = HttpUtility.UrlEncode(cmConfiguration.EncryptedSearchApiKey);
			var farmName = HttpUtility.UrlEncode(cmConfiguration.FarmName);
			var coveoAdminUsername = HttpUtility.UrlEncode(cmConfiguration.SitecoreUsername);
			var coveoAdminPassword = HttpUtility.UrlEncode(cmConfiguration.EncryptedSitecorePassword);

			var client = new HttpClient { BaseAddress = new Uri(hostCD) };
			CoveoIsActivatedStatus isActivatedStatus;

			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/Utilities/ConfigureCdAndActivateCoveo.aspx?organizationId={organizationId}&apiKey={apiKey}&searchApiKey={searchApiKey}&farmName={farmName}&adminUsername={coveoAdminUsername}&adminPassword={coveoAdminPassword}"))
			{
				using (var response = await client.SendAsync(request).ConfigureAwait(false))
				{
					var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
					isActivatedStatus = (CoveoIsActivatedStatus)JsonConvert.DeserializeObject(responseContent, typeof(CoveoIsActivatedStatus));
				}
			}

			return isActivatedStatus.IsActivated;
		}

		private async Task<bool> ActivateCdCustomization()
		{
			var hostCD = Environment.GetEnvironmentVariable("HOST_CD");
			var client = new HttpClient { BaseAddress = new Uri(hostCD) };
			CoveoIsActivatedStatus isActivatedStatus;

			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/Utilities/ActivateCoveoCustomization.aspx"))
			{
				using (var response = await client.SendAsync(request).ConfigureAwait(false))
				{
					var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
					isActivatedStatus = (CoveoIsActivatedStatus)JsonConvert.DeserializeObject(responseContent, typeof(CoveoIsActivatedStatus));
				}
			}

			return isActivatedStatus.IsActivated;
		}

		private void ActivateCmCustomization()
		{
			string[] foundationFilesToRename = Directory.GetFiles("C:\\inetpub\\wwwroot\\App_Config\\include\\Foundation", "*.demo", SearchOption.AllDirectories);
			RenameFiles(foundationFilesToRename, "demo");

			string[] projectFilesToRename = Directory.GetFiles("C:\\inetpub\\wwwroot\\App_Config\\include\\Project", "*.demo", SearchOption.AllDirectories);
			RenameFiles(projectFilesToRename, "demo");
		}

		private void RenameFiles(string[] filesToRename, string extension)
		{
			foreach (string fileName in filesToRename)
			{
				FileInfo fileInfo = new FileInfo(fileName);
				fileInfo.MoveTo(fileName.Replace($".config.{extension}", ".config"));
			}
		}
	}
}
