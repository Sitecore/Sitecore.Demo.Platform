using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitecore.Demo.Init.Model;
using Sitecore.Demo.Init.Services;

namespace Sitecore.Demo.Init.Jobs
{
	class ActivateCoveo : CoveoTaskBase
	{
		private readonly WaitForSitecoreToStart waitForSitecoreToStart;

		public ActivateCoveo(InitContext initContext)
			: base(initContext)
		{
			waitForSitecoreToStart = new WaitForSitecoreToStart(initContext);
		}

		public async Task Run()
		{
			if (this.IsCompleted() && !this.HaveSettingsChanged())
			{
				Log.LogWarning($"{TaskName} is already complete and settings have not changed. It will not execute this time.");
				return;
			}

			if (!AreCoveoEnvironmentVariablesSet())
			{
				Log.LogWarning($"{TaskName} skipped as COVEO_* environment variables are not configured.");
				return;
			}

			await Start(TaskName);

			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");

			Log.LogInformation($"{TaskName}() started on {hostCM}");

			await waitForSitecoreToStart.Run();

			var authenticatedClient = await GetAuthenticatedClient(hostCM);

			bool configurationSucceded = await ConfigureCm(hostCM, authenticatedClient);
			if (!configurationSucceded)
			{
				Log.LogError($"{TaskName}() failed while configuring the CM.");
				return;
			}

			if (await IsCoveoActivatedOnCm(hostCM, authenticatedClient) == false)
			{
				bool cmActivationSucceded = await ActivateCm(hostCM, authenticatedClient);
				if (!cmActivationSucceded)
				{
					Log.LogError($"{TaskName}() failed while activating the CM.");
					return;
				}

				await waitForSitecoreToStart.Run();

				bool customizationActivationSucceded = await ActivateCoveoCustomizations(hostCM);
				if (!customizationActivationSucceded)
				{
					Log.LogError($"{TaskName}() failed while activating the CM customizations.");
					return;
				}

				Log.LogInformation($"{TaskName}() finished on {hostCM}.");
			}
			else
			{
				Log.LogInformation($"{TaskName}() finished on {hostCM}. Coveo is already activated on CM.");
			}

			if (!ShouldActivateCoveoOnCd())
			{
				await StopTaskWithSuccess($"{TaskName}() complete. There is no CD to configure and activate.");
				return;
			}

			var hostCD = Environment.GetEnvironmentVariable("HOST_CD");

			Log.LogInformation($"{TaskName}() started on {hostCD}");

			await waitForSitecoreToStart.RunCD();

			bool isCoveoAlreadyActivatedOnCd = await IsCoveoActivatedOnCd(hostCD);

			await waitForSitecoreToStart.Run();

			CoveoConfigurationResponse cmConfiguration = await GetCmConfiguration(hostCM, authenticatedClient);
			if (cmConfiguration == null)
			{
				Log.LogError($"{TaskName}() failed while getting the CM contiguration.");
				return;
			}

			bool cdActivationSucceded = await ConfigureAndActivateCd(hostCD, cmConfiguration);
			if (!cdActivationSucceded)
			{
				Log.LogError($"{TaskName}() failed while configuring and activating the CD.");
				return;
			}

			if (!isCoveoAlreadyActivatedOnCd)
			{
				await waitForSitecoreToStart.RunCD();

				bool customizationActivationSucceded = await ActivateCoveoCustomizations(hostCD);
				if (!customizationActivationSucceded)
				{
					Log.LogError($"{TaskName}() failed while activating the CD customizations.");
					return;
				}

				Log.LogInformation($"{TaskName}() finished on {hostCD}.");
			}
			else
			{
				Log.LogInformation($"{TaskName}() finished on {hostCD}. Coveo customizations are already activated on CD.");
			}

			await StopTaskWithSuccess($"{TaskName}() complete");
		}

		private async Task StopTaskWithSuccess(string message)
		{
			await Stop(TaskName);
			Log.LogInformation(message);
		}

		private async Task<WebClient> GetAuthenticatedClient(string hostCM)
		{
			var hostID = Environment.GetEnvironmentVariable("HOST_ID");
			var sitecoreAdminUserName = Environment.GetEnvironmentVariable("ADMIN_USER_NAME").Replace("sitecore\\", string.Empty);
			var sitecoreAdminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

			return await new SitecoreLoginService(Log).GetSitecoreClient(hostCM, hostID, sitecoreAdminUserName, sitecoreAdminPassword);
		}

		private async Task<bool> IsCoveoActivatedOnCm(string hostCM, WebClient authenticatedClient)
		{
			var response = await authenticatedClient.DownloadStringTaskAsync($"{hostCM}/coveo/api/config/v1/activate");
			Log.LogInformation($"{response}");
			CoveoIsActivatedStatus isActivatedStatus = (CoveoIsActivatedStatus)JsonConvert.DeserializeObject(response, typeof(CoveoIsActivatedStatus));
			return isActivatedStatus.IsActivated;
		}

		private async Task<bool> ConfigureCm(string hostCM, WebClient authenticatedClient)
		{
			Log.LogInformation("Starting Coveo CM configuration...");

			var coveoAdminUserName = CoveoAdminUserName.Replace("\\", "\\\\");

			string requestBody = $"{{" +
				$"  \"Organization\": {{" +
				$"    \"OrganizationId\": \"{CoveoOrganizationId}\"," +
				$"    \"ApiKey\": \"{CoveoApiKey}\"," +
				$"    \"SearchApiKey\": \"{CoveoSearchApiKey}\"," +
				$"    \"PlatformEndpointUrl\": \"{CoveoPlatformEndpointUrl}\"," +
				$"    \"IndexingEndpointUrl\": \"{CoveoIndexingEndpointUrl}\"," +
				$"    \"UsageAnalyticsEndpointUrl \": \"{CoveoUsageAnalyticsEndpointUrl}\"" +
				$"  }}," +
				$"  \"SitecoreCredentials\": {{" +
				$"    \"Username\": \"{coveoAdminUserName}\"," +
				$"    \"Password\": \"{CoveoAdminPassword}\"" +
				$"  }}," +
				$"  \"DocumentOptions\": {{" +
				$"    \"BodyIndexing\": \"Rich\"," +
				$"    \"IndexPermissions\": false" +
				$"  }}," +
				$"  \"Farm\": {{" +
				$"    \"Name\": \"{CoveoFarmName}\"" +
				$"  }}" +
				$"}}";

			try
			{
				authenticatedClient.Headers.Add("Content-Type", "application/json");
				string response = await authenticatedClient.UploadStringTaskAsync(
					$"{hostCM}/coveo/api/config/v1/configure",
					"PUT",
					requestBody);
			}
			catch (WebException exception)
			{
				StreamReader reader = new StreamReader(exception.Response.GetResponseStream());
				string response = reader.ReadToEnd();

				Log.LogInformation("Coveo configuration failed.\n\n{0}", response);
				return false;
			}

			Log.LogInformation("Coveo CM configuration done.");
			return true;
		}

		private async Task<bool> ActivateCm(string hostCM, WebClient authenticatedClient)
		{
			Log.LogInformation("Starting Coveo CM activation...");

			try
			{
				await authenticatedClient.UploadStringTaskAsync(
					$"{hostCM}/coveo/api/config/v1/activate",
					"POST",
					"");
			}
			catch (WebException exception)
			{
				StreamReader reader = new StreamReader(exception.Response.GetResponseStream());
				string response = reader.ReadToEnd();

				Log.LogInformation("Coveo activation failed.\n\n{0}", response);
				return false;
			}

			Log.LogInformation("Coveo CM activation done.");
			return true;
		}

		private async Task<bool> ActivateCoveoCustomizations(string host)
		{
			Log.LogInformation($"Starting Coveo customization activation on {host} ...");

			using var client = new HttpClient { BaseAddress = new Uri(host) };
			CoveoIsActivatedStatus isActivatedStatus;

			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/Utilities/ActivateCoveoCustomization.aspx"))
			{
				using (var response = await client.SendAsync(request))
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					Log.LogInformation($"{response.StatusCode} {responseContent}");
					isActivatedStatus = (CoveoIsActivatedStatus)JsonConvert.DeserializeObject(responseContent, typeof(CoveoIsActivatedStatus));
				}
			}

			Log.LogInformation($"Coveo customization activation on {host} done.");
			return isActivatedStatus.IsActivated;
		}

		private bool ShouldActivateCoveoOnCd()
		{
			var skipWarmupCd = Environment.GetEnvironmentVariable("SKIP_WARMUP_CD");
			return skipWarmupCd != "true";
		}

		private async Task<bool> IsCoveoActivatedOnCd(string hostCD)
		{
			using var client = new HttpClient { BaseAddress = new Uri(hostCD) };
			CoveoIsActivatedStatus isActivatedStatus;

			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/Utilities/IsCoveoActivated.aspx"))
			{
				using (var response = await client.SendAsync(request))
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					Log.LogInformation($"{response.StatusCode} {responseContent}");
					isActivatedStatus = (CoveoIsActivatedStatus)JsonConvert.DeserializeObject(responseContent, typeof(CoveoIsActivatedStatus));
				}
			}

			return isActivatedStatus.IsActivated;
		}

		private async Task<CoveoConfigurationResponse> GetCmConfiguration(string hostCM, WebClient authenticatedClient)
		{
			var response = await authenticatedClient.DownloadStringTaskAsync($"{hostCM}/Utilities/GetCoveoConfiguration.aspx");
			Log.LogInformation($"{response}");
			CoveoConfigurationResponse configuration = (CoveoConfigurationResponse)JsonConvert.DeserializeObject(response, typeof(CoveoConfigurationResponse));

			if (string.IsNullOrEmpty(configuration.EncryptedApiKey) || string.IsNullOrEmpty(configuration.EncryptedSearchApiKey) || string.IsNullOrEmpty(configuration.EncryptedSitecorePassword))
			{
				return null;
			}

			return configuration;
		}

		private async Task<bool> ConfigureAndActivateCd(string hostCD, CoveoConfigurationResponse cmConfiguration)
		{
			Log.LogInformation("Starting Coveo CD configuration and activation...");

			var organizationId = HttpUtility.UrlEncode(CoveoOrganizationId);
			var apiKey = HttpUtility.UrlEncode(cmConfiguration.EncryptedApiKey);
			var searchApiKey = HttpUtility.UrlEncode(cmConfiguration.EncryptedSearchApiKey);
			var farmName = HttpUtility.UrlEncode(CoveoFarmName);
			var coveoAdminUsername = HttpUtility.UrlEncode(CoveoAdminUserName);
			var coveoAdminPassword = HttpUtility.UrlEncode(cmConfiguration.EncryptedSitecorePassword);
			var coveoPlatformEndpointUrl = HttpUtility.UrlEncode(CoveoPlatformEndpointUrl);
			var coveoIndexingEndpointUrl = HttpUtility.UrlEncode(CoveoIndexingEndpointUrl);
			var coveoUsageAnalyticsEndpointUrl = HttpUtility.UrlEncode(CoveoUsageAnalyticsEndpointUrl);

			using var client = new HttpClient { BaseAddress = new Uri(hostCD) };
			CoveoIsActivatedStatus isActivatedStatus;

			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/Utilities/ConfigureCdAndActivateCoveo.aspx?organizationId={organizationId}&apiKey={apiKey}&searchApiKey={searchApiKey}&farmName={farmName}&adminUsername={coveoAdminUsername}&adminPassword={coveoAdminPassword}&platformEndpointUrl={coveoPlatformEndpointUrl}&indexingEndpointUrl={coveoIndexingEndpointUrl}&usageAnalyticsEndpointUrl={coveoUsageAnalyticsEndpointUrl}"))
			{
				using (var response = await client.SendAsync(request))
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					Log.LogInformation($"{response.StatusCode} {responseContent}");
					isActivatedStatus = (CoveoIsActivatedStatus)JsonConvert.DeserializeObject(responseContent, typeof(CoveoIsActivatedStatus));
				}
			}

			Log.LogInformation("Coveo CD configuration and activation done.");
			return isActivatedStatus.IsActivated;
		}
	}
}
