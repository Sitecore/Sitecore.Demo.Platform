using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Sitecore.Demo.Init.Model;
using Sitecore.Demo.Init.Services;
using Microsoft.Extensions.Logging;

namespace Sitecore.Demo.Init.Jobs
{
	class ActivateCoveo : TaskBase
	{
		private readonly WaitForSitecoreToStart waitForSitecoreToStart;

		public ActivateCoveo(InitContext initContext)
			: base(initContext)
		{
			waitForSitecoreToStart = new WaitForSitecoreToStart(initContext);
		}

		public async Task Run()
		{
			await Start(nameof(ActivateCoveo));

			if (!AreCoveoEnvironmentVariablesSet())
			{
				await StopTaskWithSuccess("ActivateCoveo() skipped as COVEO_* environment variables are not configured.");
				return;
			}

			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");

			Log.LogInformation($"ActivateCoveo() started on {hostCM}");

			await waitForSitecoreToStart.Run();

			var authenticatedClient = await GetAuthenticatedClient(hostCM);

			if (await IsCoveoActivatedOnCm(hostCM, authenticatedClient) == false)
			{
				bool configurationSucceded = await ConfigureCm(hostCM, authenticatedClient);
				if (!configurationSucceded)
				{
					await StopTaskWithError("ActivateCoveo() failed while configuring the CM.");
					return;
				}

				bool activationSucceded = await ActivateCm(hostCM, authenticatedClient);
				if (!activationSucceded)
				{
					await StopTaskWithError("ActivateCoveo() failed while activating the CM.");
					return;
				}

				await waitForSitecoreToStart.Run();

				bool customizationActivationSucceded = await ActivateCoveoCustomizations(hostCM);
				if (!customizationActivationSucceded)
				{
					await StopTaskWithError("ActivateCoveo() failed while activating the CM customizations.");
					return;
				}

				Log.LogInformation($"ActivateCoveo() finished on {hostCM}.");
			}
			else
			{
				Log.LogInformation($"ActivateCoveo() finished on {hostCM}. Coveo is already activated on CM.");
			}

			if (!ShouldActivateCoveoOnCd())
			{
				await StopTaskWithSuccess("ActivateCoveo() complete");
				return;
			}

			var hostCD = Environment.GetEnvironmentVariable("HOST_CD");

			Log.LogInformation($"ActivateCoveo() started on {hostCD}");

			await waitForSitecoreToStart.RunCD();

			if (await IsCoveoActivatedOnCd(hostCD) == false)
			{
				await waitForSitecoreToStart.Run();

				CoveoConfigurationResponse cmConfiguration = await GetCmConfiguration(hostCM, authenticatedClient);

				bool configurationSucceded = await ConfigureAndActivateCd(hostCD, cmConfiguration);
				if (!configurationSucceded)
				{
					await StopTaskWithError("ActivateCoveo() failed while configuring and activating the CD.");
					return;
				}

				await waitForSitecoreToStart.RunCD();

				bool customizationActivationSucceded = await ActivateCoveoCustomizations(hostCD);
				if (!customizationActivationSucceded)
				{
					await StopTaskWithError("ActivateCoveo() failed while activating the CD customizations.");
					return;
				}

				Log.LogInformation($"ActivateCoveo() finished on {hostCD}.");
			}
			else
			{
				Log.LogInformation($"ActivateCoveo() finished on {hostCD}. Coveo is already activated on CD.");
			}

			await StopTaskWithSuccess("ActivateCoveo() complete");
		}

		private async Task StopTaskWithError(string message)
		{
			await StopTask();
			Log.LogError(message);
		}

		private async Task StopTaskWithSuccess(string message)
		{
			await StopTask();
			Log.LogInformation(message);
		}

		private async Task StopTask()
		{
			await Stop(nameof(ActivateCoveo));
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

			var organizationId = Environment.GetEnvironmentVariable("COVEO_ORGANIZATION_ID");
			var apiKey = Environment.GetEnvironmentVariable("COVEO_API_KEY");
			var searchApiKey = Environment.GetEnvironmentVariable("COVEO_SEARCH_API_KEY");
			var farmName = Environment.GetEnvironmentVariable("COVEO_FARM_NAME");
			var coveoAdminUserName = Environment.GetEnvironmentVariable("COVEO_ADMIN_USER_NAME").Replace("\\", "\\\\");
			var coveoAdminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

			string requestBody = $"{{" +
				$"  \"Organization\": {{" +
				$"    \"OrganizationId\": \"{organizationId}\"," +
				$"    \"ApiKey\": \"{apiKey}\"," +
				$"    \"SearchApiKey\": \"{searchApiKey}\"" +
				$"  }}," +
				$"  \"SitecoreCredentials\": {{" +
				$"    \"Username\": \"{coveoAdminUserName}\"," +
				$"    \"Password\": \"{coveoAdminPassword}\"" +
				$"  }}," +
				$"  \"DocumentOptions\": {{" +
				$"    \"BodyIndexing\": \"Rich\"," +
				$"    \"IndexPermissions\": false" +
				$"  }}," +
				$"  \"Farm\": {{" +
				$"    \"Name\": \"{farmName}\"" +
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
			return skipWarmupCd == "false";
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
			return configuration;
		}

		private async Task<bool> ConfigureAndActivateCd(string hostCD, CoveoConfigurationResponse cmConfiguration)
		{
			Log.LogInformation("Starting Coveo CD configuration and activation...");

			var organizationId = HttpUtility.UrlEncode(Environment.GetEnvironmentVariable("COVEO_ORGANIZATION_ID"));
			var apiKey = HttpUtility.UrlEncode(cmConfiguration.EncryptedApiKey);
			var searchApiKey = HttpUtility.UrlEncode(cmConfiguration.EncryptedSearchApiKey);
			var farmName = HttpUtility.UrlEncode(Environment.GetEnvironmentVariable("COVEO_FARM_NAME"));
			var coveoAdminUsername = HttpUtility.UrlEncode(Environment.GetEnvironmentVariable("COVEO_ADMIN_USER_NAME"));
			var coveoAdminPassword = HttpUtility.UrlEncode(cmConfiguration.EncryptedSitecorePassword);

			using var client = new HttpClient { BaseAddress = new Uri(hostCD) };
			CoveoIsActivatedStatus isActivatedStatus;

			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/Utilities/ConfigureCdAndActivateCoveo.aspx?organizationId={organizationId}&apiKey={apiKey}&searchApiKey={searchApiKey}&farmName={farmName}&adminUsername={coveoAdminUsername}&adminPassword={coveoAdminPassword}"))
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
