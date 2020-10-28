using System;
using System.Net;

namespace Sitecore.Demo.Init.Extensions
{
	using System.Linq;

	public class CookieWebClient : WebClient
	{
		[System.Security.SecuritySafeCritical]
		public CookieWebClient()
			: base()
		{
		}

		public WebHeaderCollection LastResponseHeaders { get; private set; }

		public Uri LastResponseUri { get; private set; }

		public bool AllowAutoRedirect { get; set; } = true;

		protected override WebResponse GetWebResponse(WebRequest request)
		{
			WebResponse response = null;
			try
			{
				response = base.GetWebResponse(request);
			}
			catch (WebException e)
			{
				if (e.Message.Contains("302"))
				{
					response = e.Response;
				}
			}

			// Allow secure cookies on http://cm and http://id
			if (response?.Headers["Set-Cookie"] != null && request.RequestUri.Scheme != "https")
			{
				var container = new CookieContainer();
				container.SetCookies(new Uri("https://localhost"), response?.Headers["Set-Cookie"]);
				var cookies = container.GetCookies(new Uri("https://localhost"));

				foreach (Cookie cookie in cookies)
				{
					if (cookie.Secure)
					{
						cookieContainer.Add(new Cookie(cookie.Name, cookie.Value, "/", request.RequestUri.Host));
					}
				}
			}

			LastResponseUri = response?.ResponseUri;
			LastResponseHeaders = response?.Headers;
			return response;
		}

		public CookieContainer cookieContainer = new CookieContainer();

		protected override WebRequest GetWebRequest(Uri myAddress)
		{
			var request = (HttpWebRequest)base.GetWebRequest(myAddress);
			request.AllowAutoRedirect = this.AllowAutoRedirect;
			request.Timeout = 1000 * 60 * 10; // 10 minutes

			if (request is HttpWebRequest)
			{
				(request as HttpWebRequest).CookieContainer = cookieContainer;
			}

			return request;
		}
	}
}
