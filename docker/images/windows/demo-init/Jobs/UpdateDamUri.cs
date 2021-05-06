using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Sitecore.Demo.Init.Model;
using Sitecore.Demo.Init.Extensions;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Sitecore.Demo.Init.Jobs
{
	class UpdateDamUri : TaskBase
	{
		public UpdateDamUri(InitContext initContext)
			: base(initContext)
		{
		}

		public static string damUrl = Environment.GetEnvironmentVariable("DAM_URL");

		//TODO: Implement recursive approach to find Fitness Event items by template - Out of time right now
		public static List<string> itemIdList = new List<string>{
			//Alberta
			"667FB3A0-CCB7-43C5-B814-798F7B1456E0",
			"6AB77073-7D7A-4907-9A40-03938B18B116",
			"4CCD011A-5B82-4E0F-A180-08EDA3B031B6",
			"46F73521-5D1B-47D5-A684-F34C433FF2DB",
			"6C0568FE-9674-46B3-8A70-A06D030A5672",
			"DE2A0CC5-BCB7-4012-AA92-80F5D55DB0C4",
			"069A1AEC-0179-48E1-BA89-320A6378D961",
			"FD4750FE-DF77-40E4-AC61-5E1C69D8A963",
			"D6ACFB43-2012-4E63-9AED-4094BA4C2085",
			"C89054E8-1C39-4B88-9608-DE9DC94664EA",
			"BD44196C-1962-476A-AB90-FAAB0F8C78A5",
			"DC596A7E-D5E6-49A2-A018-EB7DEC69CC99",
			"0B77FA28-6202-442B-9A24-65D720E4D961",
			"AC39F140-95D0-4B3A-A7D1-B8CFF3C3614B",
			//BC
			"47DAB064-4C98-4044-874C-E8C4A43AD407", 
			"A43EBACB-80BE-4E9D-9E04-CAFC64CE92E7", 
			"0CE70096-BC5F-46DC-9F5D-202FEE345205",
			"44D8752D-8C1F-432D-B2EA-C1826A4AD3A2",
			"7EDBA7FF-7584-4625-BECF-A1A2986101BC",
			"BBEAA596-267F-4E71-BBA3-C2FE21C36325",
			"1B51D656-D7D0-4AA7-A075-87AD90E55349",
			"38248F8D-10CB-4046-95A1-685E7E515C6F",
			"9D3D853D-9355-4733-805C-7A323174EA83",
			"3ECF99D9-55FD-45DA-8ECE-E653C2080C07",
			"C98A401B-9D93-447E-AA40-C145909696B7",
			"F8909F45-3866-4919-A74B-36DFA60DEEF1",
			"3B07F23A-DCF9-40C8-9F36-B9F94932F441",
			"74FED3E0-3461-4B84-B5F5-21714E2C3E53",
			"456D3415-49CE-4EA0-BB57-76353CBA1D2F",
			//Manitoba
			"7DCA6D2B-4AA4-40EA-917C-5B660E9A11A5",
			"4CA07D91-ECA4-4175-B2FD-3CDAAD0BFE91",
			"D4F99C5C-3CEF-41DC-8A01-3DDB08708549",
			"4BC88AD7-301B-4B38-85BC-91DB00F575EB",
			"C565829C-0070-446B-8738-0EEE9B25F5FB",
			"883CA620-3ED2-4A4E-8946-E5F79BB7AB3A",
			"5C17BEFE-ADE3-4942-8BEE-1D7E36A0A012",
			"8552D890-999C-489A-B2CA-B2AA103ED781",
			"77DC862B-F393-4BD7-B03C-BF029FB20470",
			"F4246DD7-2646-4FC8-AA4E-1A8C6ED2F8A1",
			"20F06937-B73E-47E0-9191-C2342A1A2628",
			"520034A8-3D83-4BCE-AA18-0DFC87C705E5",
			"C62B5A54-F04F-4D56-893A-CB98A8AAB30B",
			"B129A768-85D9-469E-BD38-04F19E707696",
			"687AD651-4DAA-4F6D-8E48-F21545777A89",
			//New Brunswick
			"7E8A0209-E971-4FD0-9590-FD221B095890",
			"E567C2ED-4088-4240-8D22-A6D15BB5E725",
			"3EFE2D9F-592C-437D-8FFF-FA80DCDE5B80",
			"C8B6CBBC-AF82-469C-8A46-32341033BED5",
			"B52C91B4-5042-4F80-AB82-A0F1F322F957",
			"6370B60E-468F-4642-876E-20CA08A56FBE",
			//Newfoundland and Labrador
			"EFCD8569-1FAC-408E-9751-AF6C69393C52",
			"AE041ED0-99D5-4EE3-B2F6-AA3234F44EEF",
			"BBB0F0E9-6BBD-45D8-91CF-E51923B325CE",
			"BADEAE9B-1DA8-4398-9A2E-888A1BFBCB6D",
			"DBA8BB2E-9374-4B51-8D7D-8B44C4BF754F",
			"2AA39C39-8A45-46C7-8695-D19A6E76B2A4",
			"7917921C-868A-4F47-A7DB-8196639DE0FF",
			//Northwest Territories
			"5800BE95-AA4B-4034-9656-0D189C5571F5",
			"322E5B67-C120-4F3E-87F7-229E26EC2EE3",
			"A2AE49E9-B809-4007-A8E1-C3A298E4F698",
			"5071EDC3-E43E-4178-89BE-00842DB6C91E",
			"DD388E6C-C464-43A2-B9DA-AC956E43D333",
			//Nova Scotia
			"749CD9A3-24EB-4178-8DB6-01A8A4922E75",
			"B98CF43A-A35A-4D77-9DB1-6CE3C739DE2E",
			"D203C6F8-8F4F-4C53-8E5C-C6F20D793F0A",
			"FF47F1B8-8928-42EA-928C-F69DBBB4EF20",
			//Nunavut
			"A5464585-1777-4716-9911-4A455FAB1EAB",
			"78E44ADE-1919-45D4-B5B7-8BC2F1127EDE",
			"10A30E0A-0883-45B1-AC22-05BB4E87E04D",
			"CAC8E69B-9767-439C-828D-E9D279299935",
			"7C7EF6B1-B91A-444A-A04B-B622A8ED8FD8",
			"21A50A3E-936D-4F5E-8846-BEF42495BDCE",
			"BB582910-827F-4A2C-B633-FA226F01D792",
			"5E5284D8-44B2-45BC-A151-6C97467C0328",
			"8F8622A5-C46C-43ED-BFD0-CA08D302A877",
			"53D072D4-8758-430B-8B4D-9509C23C66E8",
			"21C029DD-1E02-4297-87C4-DBAB9E5FE466",
			"1DFCD0D3-4750-4CC6-AA94-AAEA700C7642",
			"0C5E7E25-D90A-47DA-9DA4-C403513E0DC5",
			"1973BEB9-308A-4CE3-BE9D-C449AAA30241",
			//Ontario
			"69FA963D-ACAB-4C99-A4ED-3A448F0A7920",
			"1BB98949-3DBD-4CBA-AB0E-2D7543EA94FC",
			"3AFE3C05-5B58-4078-A23A-CB99617F0E19",
			"181A7A93-225A-4917-87C4-B345973D147C",
			"B65DE05B-8A74-446C-B8E5-5D6F3557CD03",
			"40144D08-254C-49F5-AAA6-53049EC8361F",
			"8ECE0DE5-9E08-41F6-98A4-F2AB8BBA33DC",
			"8BFAE67F-71F4-410E-AEC4-2FAB1C1AA3BF",
			"88073DA3-9834-4E0E-96AB-1DD3AA13EF73",
			"54A20A81-20AD-4A0E-BFD3-65D0AE074660",
			"FCCC9769-FCA1-4ACA-9FB0-2BE937C137FE",
			"783BA181-7711-48B3-9615-A91ABDF05BC2",
			//Quebec
			"287A2045-2B2C-4A07-B482-7887688B5EFA",
			"3C2D432D-AE79-4B00-8DBD-6126E5522B23",
			"A4CC437D-F201-4DCC-8EC8-5583393BA3F1",
			"1001D7BA-8AA7-4FE1-B66A-8C78865CD281",
			"894563FE-3146-447A-A012-CC235CFF6990",
			"89ACEF0D-07DE-4AB4-90FA-61EA47C2E132",
			"67E2C1CE-9759-4775-8871-5B104DD36BCB",
			"464CE1A8-1427-4388-8165-4D05C86FA8BE",
			//Saskatchewan
			"A518297F-B0B9-419E-A58E-BCFC719044CB",
			"E95FD5F5-DBA4-429E-B125-B5E845AB82F7"
		};

		public async Task Run()
		{
			if (this.IsCompleted())
			{
				Log.LogWarning($"{this.GetType().Name} is already complete, it will not execute this time");
				return;
			}
			
			if (string.IsNullOrWhiteSpace(damUrl))
			{
				Log.LogWarning($"{this.GetType().Name} has been skipped, it does not execute if the DAM_URL variable is not passed to the Init container");
			 	return;
			}

			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");
			var user = Environment.GetEnvironmentVariable("ADMIN_USER_NAME").Replace("sitecore\\", string.Empty);
			var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

			Log.LogInformation($"UpdateDamUri() started on {hostCM}");
			using var client = new HttpClient { BaseAddress = new Uri(hostCM) };
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/sitecore/api/ssc/auth/login")
			{
				Content = new StringContent($"{{\"domain\":\"sitecore\",\"username\":\"{user}\",\"password\":\"{password}\"}}", Encoding.UTF8, "application/json")
			};

			var response = await client.SendAsync(request);
			var contents = await response.Content.ReadAsStringAsync();
			Log.LogInformation($"{response.StatusCode} {contents}");
			var token = JsonConvert.DeserializeObject<SscLoginResponse>(contents).Token;

		  	UpdateValues(hostCM, token);

		  	Log.LogInformation("UpdateDamUri() complete");
			await Complete();
		}

		private void UpdateValues(string hostCM, string token)
		{
			foreach (var itemId in itemIdList)
			{
				var existingFieldValue = ReadItemField(hostCM, token, itemId);

				if (string.IsNullOrWhiteSpace(existingFieldValue))
					continue;

				var updatedDamHost = GetUpdatedDamHost(existingFieldValue);

				if (string.IsNullOrWhiteSpace(updatedDamHost))
					continue;

				var escapedUpdatedDamHost = updatedDamHost.Replace("\"", "\\\"");

				UpdateItemField(hostCM, token, itemId, escapedUpdatedDamHost);
			}
		}

		private string ReadItemField(string hostCM, string token, string itemId)
		{	
			try
            {
				var cookieClient = new CookieWebClient();
				cookieClient.Encoding = System.Text.Encoding.UTF8;
				cookieClient.Headers.Add("token", token);
				cookieClient.Headers.Add("Content-Type", "application/json");

				var stringResponse = cookieClient.DownloadString(
					new Uri(hostCM + $"/sitecore/api/ssc/item/{itemId}?database=master&fields=image"));

				var jsonDictionarySet = JsonConvert.DeserializeObject<Dictionary<String, String>>(stringResponse);
				return jsonDictionarySet?.FirstOrDefault(x => x.Key == "image").Value;
			}
            catch (Exception ex)
            {
                Log.LogError($"Failed to read item field, itemId: {itemId}", ex);
                return null;
            }
  		}

		private string GetUpdatedDamHost(string existingFieldValue)
		{	
			if (string.IsNullOrWhiteSpace(existingFieldValue) || !existingFieldValue.Contains("stylelabs-content-id"))
				return string.Empty;	

			var damUri = new System.Uri(damUrl);
			var damHost = damUri?.Host;

			if (string.IsNullOrWhiteSpace(damHost))
				return string.Empty;

			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(existingFieldValue);
			var imageSrc = htmlDoc?.DocumentNode?.ChildNodes
				?.FirstOrDefault()?.Attributes
				?.FirstOrDefault(i => i?.Name == "src");
			var imageSrcUri = new System.Uri(imageSrc.Value);
			
			if (imageSrcUri == null)
				return string.Empty;	

			var imageSrcHost = imageSrcUri.Host;

			if (imageSrcHost == null)
				return string.Empty;	

			return existingFieldValue?.Replace(imageSrcHost, damHost);
		}

		private void UpdateItemField(string hostCM, string token, string itemId, string escapedUpdatedDamHost)
		{	
			try
            {
				var cookieClient = new CookieWebClient();
				cookieClient.Encoding = System.Text.Encoding.UTF8;
				cookieClient.Headers.Add("token", token);
				cookieClient.Headers.Add("Content-Type", "application/json");

				cookieClient.UploadDataAsync(
					new Uri(hostCM + $"/sitecore/api/ssc/item/{itemId}?database=master"),
					"PATCH",
					System.Text.Encoding.UTF8.GetBytes($"{{\"image\": \"{escapedUpdatedDamHost}\" }}"));
			}
            catch (Exception ex)
            {
				Log.LogError($"Failed to update item field, itemId: {itemId}", ex);
            }
		}
	}
}
