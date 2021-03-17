using System;
using System.Net;
using System.Text;
using System.Web;
using Sitecore.Demo.Init.Extensions;
using Microsoft.Extensions.Logging;

namespace Sitecore.Demo.Init.Services
{
	public class SitecoreLoginService
	{
		private readonly ILogger logger;

		public SitecoreLoginService(ILogger logger)
		{
			this.logger = logger;
		}

		public WebClient GetSitecoreClient(string baseUrl, string idBaseUrl, string user, string password)
		{
			string uri = string.Format(
				"{0}/identity/externallogin?authenticationType=SitecoreIdentityServer&ReturnUrl=%2fidentity%2fexternallogincallback%3fReturnUrl%3d%26sc_site%3dshell%26authenticationSource%3dDefault&sc_site=shell",
				baseUrl);
			var webClient = new CookieWebClient();
			webClient.BaseAddress = baseUrl;

			// Disable auto redirect as ID may return an address that will not resolve within Docker network
			webClient.AllowAutoRedirect = false;

			// Initiate login
			webClient.UploadData(uri, new byte[0]);

			// Go to /connect/authorize?client_id=Sitecore&response_type=code...
			webClient.DownloadString(idBaseUrl + new Uri(webClient.LastResponseHeaders["Location"]).PathAndQuery);
			var response = webClient.DownloadString(idBaseUrl + new Uri(webClient.LastResponseHeaders["Location"]).PathAndQuery);

			string token = ExtractParameter(response, "__RequestVerificationToken", "\"");
			string queryString = webClient.LastResponseUri.Query;
			var queryDictionary = HttpUtility.ParseQueryString(queryString);

			string postData =
				$"AccountPrefix=sitecore\\&ReturnUrl={HttpUtility.UrlEncode(queryDictionary["ReturnUrl"])}&__RequestVerificationToken={token}&ActiveTab=&AdvancedOptionsStartUrl=%2Fsitecore%2Fshell%2Fdefault.aspx&Username={user}&Password={password}&button=login&RememberLogin=true";

			webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

			// Submit a form with user and password. Identity Server returns different Uris depending on host name, that is why such condition.
			webClient.UploadData(idBaseUrl + webClient.LastResponseUri.PathAndQuery, "POST", Encoding.ASCII.GetBytes(postData));
			if (Uri.TryCreate(webClient.LastResponseHeaders["Location"], UriKind.Absolute, out Uri result))
			{
				// This fixes weird issues with URL encoding on Linux
				response = webClient.DownloadString(idBaseUrl + result.PathAndQuery.Replace("%25", "%").Replace("%3F", "?"));
			}
			else
			{
				response = webClient.DownloadString(idBaseUrl + webClient.LastResponseHeaders["Location"]);
			}

			var signInData =
				$"code={ExtractParameter(response, "code", "'")}&id_token={ExtractParameter(response, "id_token", "'")}&access_token={ExtractParameter(response, "access_token", "'")}&token_type={ExtractParameter(response, "token_type", "'")}&expires_in={ExtractParameter(response, "expires_in", "'")}&scope={ExtractParameter(response, "scope", "'")}&state={ExtractParameter(response, "state", "'")}&session_state={ExtractParameter(response, "session_state", "'")}";
			webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

			logger.LogInformation(signInData);

			// Send token to /identity/signin
			webClient.UploadData(
				baseUrl + "/identity/signin",
				"POST",
				Encoding.ASCII.GetBytes(signInData));

			// Get /externallogincallback?ReturnUrl=&sc_site=shell&authenticationSource=Default
			webClient.DownloadString(webClient.LastResponseHeaders["Location"]);

			// Test that it worked
			response = webClient.DownloadString("/sitecore/shell");

			logger.LogInformation(response.Substring(0, 100));

			webClient.AllowAutoRedirect = true;

			return webClient;
		}

		private string ExtractParameter(string s, string name, string delimiter)
		{
			const string valueToken = "value";

			int viewStateNamePosition = s.IndexOf(name, StringComparison.Ordinal);
			int viewStateValuePosition = s.IndexOf(valueToken, viewStateNamePosition, StringComparison.Ordinal);

			int viewStateStartPosition = viewStateValuePosition + valueToken.Length + 2;
			int viewStateEndPosition = s.IndexOf(delimiter, viewStateStartPosition, StringComparison.Ordinal);

			return HttpUtility.UrlEncode(
				s.Substring(viewStateStartPosition, viewStateEndPosition - viewStateStartPosition));
		}
	}
}
