using System;

namespace Sitecore.Demo.Init.Jobs
{
	public class CoveoTaskBase : TaskBase
	{
		private const string USA_COVEO_REGION = "usa";
		private const string EUROPE_COVEO_REGION = "europe";
		private const string AUSTRALIA_COVEO_REGION = "australia";

		public string CoveoOrganizationId
		{
			get
			{
				return Environment.GetEnvironmentVariable("COVEO_ORGANIZATION_ID");
			}
		}

		public string CoveoRegion
		{
			get
			{
				// This environment variable will not always be set for older deployments where only USA was supported.
				return Environment.GetEnvironmentVariable("COVEO_REGION");
			}
		}

		private string RegionUrlSuffix
		{
			get {
				switch (CoveoRegion?.ToLower()) {
					case EUROPE_COVEO_REGION:
						return "-eu";
					case AUSTRALIA_COVEO_REGION:
						return "-au";
					case USA_COVEO_REGION:
					default: // To also support older deployments that did not have a region
						return "";
				}
			}
		}

		public string CoveoPlatformEndpointUrl
		{
			get
			{
				return $"https://platform{RegionUrlSuffix}.cloud.coveo.com";
			}
		}

		public string CoveoIndexingEndpointUrl
		{
			get
			{
				return $"https://api{RegionUrlSuffix}.cloud.coveo.com/push";
			}
		}

		public string CoveoUsageAnalyticsEndpointUrl
		{
			get
			{
				return $"https://platform{RegionUrlSuffix}.cloud.coveo.com/rest/ua";
			}
		}

		public string CoveoApiKey
		{
			get
			{
				return Environment.GetEnvironmentVariable("COVEO_API_KEY");
			}
		}

		public string CoveoSearchApiKey
		{
			get
			{
				return Environment.GetEnvironmentVariable("COVEO_SEARCH_API_KEY");
			}
		}

		public string CoveoFarmName
		{
			get
			{
				return Environment.GetEnvironmentVariable("COVEO_FARM_NAME");
			}
		}

		public string CoveoAdminUserName
		{
			get
			{
				return Environment.GetEnvironmentVariable("COVEO_ADMIN_USER_NAME");
			}
		}

		public string CoveoAdminPassword
		{
			get
			{
				return Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
			}
		}

		public CoveoTaskBase(InitContext initContext)
			: base(initContext)
		{
		}

		public override string ComputeSettingsHash()
		{
			if (!AreCoveoEnvironmentVariablesSet())
			{
				return base.ComputeSettingsHash();
			}

			return CoveoOrganizationId + CoveoApiKey + CoveoSearchApiKey + CoveoFarmName + CoveoAdminUserName + CoveoAdminPassword + CoveoRegion;
		}

		protected bool AreCoveoEnvironmentVariablesSet()
		{
			// Do not add CoveoRegion to this condition to allow existing deployments (without region) to be redeployed using newer Docker images without being forced to edit on the portal and select a region.
			return !string.IsNullOrEmpty(CoveoOrganizationId) &&
			       !string.IsNullOrEmpty(CoveoApiKey) &&
			       !string.IsNullOrEmpty(CoveoSearchApiKey) &&
			       !string.IsNullOrEmpty(CoveoFarmName) &&
			       !string.IsNullOrEmpty(CoveoAdminUserName) &&
			       !string.IsNullOrEmpty(CoveoAdminPassword);
		}
	}
}
