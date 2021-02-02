using System;

namespace Sitecore.Demo.Init.Jobs
{
	public class CoveoTaskBase : TaskBase
	{
		public string CoveoOrganizationId
		{
			get {
				return Environment.GetEnvironmentVariable("COVEO_ORGANIZATION_ID");
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

			return CoveoOrganizationId + CoveoApiKey + CoveoSearchApiKey + CoveoFarmName + CoveoAdminUserName + CoveoAdminPassword;
		}

		protected bool AreCoveoEnvironmentVariablesSet()
		{
			return !string.IsNullOrEmpty(CoveoOrganizationId) &&
			       !string.IsNullOrEmpty(CoveoApiKey) &&
			       !string.IsNullOrEmpty(CoveoSearchApiKey) &&
			       !string.IsNullOrEmpty(CoveoFarmName) &&
			       !string.IsNullOrEmpty(CoveoAdminUserName) &&
			       !string.IsNullOrEmpty(CoveoAdminPassword);
		}
	}
}
