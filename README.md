# Introduction 
HabitatHome  Demo and the tools and processes in it is a Sitecore&reg; solution example built using Sitecore Experience Accelerator&trade; (SXA) on Sitecore Experience Platform&trade; (XP)  following the Helix architecture principles.


# Important Notice
<details>
## Is Habitat Home a starter kit or template solution?

No. You should not clone this repository for the purposes of starting a new Sitecore project. There are other community solutions which can be used as a starter for Helix-based Sitecore implementations. Habitat Home is intended as a **demo site demonstrating the full Sitecore platform capabilities and development best practices**.

## Is Habitat Home supported by Sitecore?

Sitecore maintains the Habitat Home example, but Habitat Home code is not supported by Sitecore Product Support Services. Please do not submit support tickets regarding Habitat.

## How can I get help with Habitat Home?

For usage questions regarding Habitat Home, installation or code, please use [Sitecore Stackexchange](https://sitecore.stackexchange.com/) or [#habitathome](https://sitecorechat.slack.com/messages/CASEB5M38) on [Sitecore Community Slack](https://www.akshaysura.com/2015/10/27/how-to-join-sitecore-slack-community-chat/). 

You can use GitHub to submit [bug reports](https://github.com/Sitecore/Sitecore.HabitatHome.Platform/issues/new?template=bug_report.md) or [feature requests](https://github.com/Sitecore/Sitecore.HabitatHome.Platform/issues/new?template=feature_request.md) for Habitat Home. Please do not submit usage questions via GitHub.

### License
Please read the LICENSE carefully prior to using the code in this repository. 
 
### Support

The code, samples and/or solutions provided in this repository are ***unsupported by Sitecore PSS***. Support is provided on a best-effort basis via GitHub issues or Slack #habitathome (see end of README for additional information).

It is assumed that you already have a working instance of Sitecore XP and all prerequisites prior to installing the demo. Support for **product installation** issues should be directed to relevant Community channels or through regular Sitecore support channels. 

### Warranty

The code, samples and/or solutions provided in this repository are for example purposes only and **without warranty (expressed or implied)**. The code has not been extensively tested and is not guaranteed to be bug free.  
</details>

# Getting Started

### Prerequisites

The latest Habitat Home demo is built to support **[Sitecore Experience Platform 9.1](https://dev.sitecore.net/Downloads/Sitecore_Experience_Platform/91/Sitecore_Experience_Platform_91_Initial_Release.aspx)** using **[Sitecore Experience Accelerator (SXA) 1.8](https://dev.sitecore.net/Downloads/Sitecore_Experience_Accelerator/18/Sitecore_Experience_Accelerator_180.aspx)**.

***In order to follow the build instructions in the README, you need to have [MSBuild Tools for Visual Studio 2017](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2017) installed.***
### Optional Modules
<details>
	
In addition to base XP 9.1 with SXA, the following optional modules are required to enable additional demo functionality:

> **We will be upgrading Data Exchange Framework to 2.1 and have not yet tested the 9.1 version of Habitat Home with DEF 2.0.1**

- ***Optional Modules***
The following optional modules enable synchronization with Dynamics CRM or Salesforce CRM (relevant account required)
  -  Base Data Exchange Framework modules
	  -  [Data Exchange Framework v2.0.1](https://dev.sitecore.net/~/media/C50B044E45FE4C4DA9E675CBEED3AA09.ashx) on [dev.sitecore.com](https://dev.sitecore.net/Downloads/Data_Exchange_Framework/2x/Data_Exchange_Framework_201.aspx) as well as relevant Providers on same page
		- [Sitecore Provider for Data Exchange Framework 2.0.1](https://dev.sitecore.net/~/media/D57A1FBB98ED4125B78D740E5B5F1772.ashx)
		- [SQL Provider for Data Exchange Framework 2.0.1](https://dev.sitecore.net/~/media/F243222B9A95497BAB6B591D39560E95.ashx)
		- [xConnect Provider for Data Exchange Framework 2.0.1](https://dev.sitecore.net/~/media/C61F1265BB494CAFA4229FC9FC704AB0.ashx) 
	  - Dynamics-specific modules
		  - Data Exchange Framework 2.0.1 Dynamics Connectors on [dev.sitecore.com](https://dev.sitecore.net/Downloads/Dynamics_CRM_Connect/2x/Sitecore_Connect_for_Microsoft_Dynamics_365_for_Sales_201.aspx)
		- [Microsoft Dynamics 365 for Sales Provider for Data Exchange Framework 2.0.1](https://dev.sitecore.net/~/media/819FB4C75CC74A8C984C343BEF7B53F1.ashx)
		- [Sitecore Connect for Microsoft Dynamics 365 for Sales 2.0.1](https://dev.sitecore.net/~/media/ADBAF4CC6736499EBA0EBA6A9767D825.ashx)
	  - Salesforce-specific modules
		  - Sitecore Connect for Salesforce CRM 2.0.1 on [dev.sitecore.com](https://dev.sitecore.net/Downloads/Salesforce_Connect/2x/Sitecore_Connect_for_Salesforce_CRM_201.aspx)
			  - [Salesforce CRM Provider for Data Exchange Framework 2.0.1](https://dev.sitecore.net/Downloads/Salesforce_Connect/2x/Sitecore_Connect_for_Salesforce_CRM_201.aspx#)
			  - [Sitecore Connect for Salesforce CRM 2.0.1](https://dev.sitecore.net/Downloads/Salesforce_Connect/2x/Sitecore_Connect_for_Salesforce_CRM_201.aspx#)

</details>

### SSL Only
The demo is configured for **HTTPS/SSL**. Please ensure that you create an HTTPS binding with appropriate self-signed certificates.
### Clone this repository
<details>

Clone the Sitecore.HabitatHome.Platform repository locally - defaults are configured for **C:\Projects\Sitecore.HabitatHome.Platform**. 


- Clone 
-- **https**:	`git clone https://github.com/Sitecore/Sitecore.HabitatHome.Platform.git` 
-- **ssh**:		`git clone git@github.com:Sitecore/Sitecore.HabitatHome.Platform.git`
</details>

#### Parameters - explained
<details>
The following is a list of default values / assumptions for settings (`cake-config.json`)

|Parameter                                  | Description 														| Default Value 
|-------------------------------------------|----------------------------------|-------------------------------------------------------
| ProjectFolder         | Location of Sitecore.HabitatHome.Platform project 									| c:\projects\Sitecore.HabitatHome.Platform |
| Website Root			|	Location of IIS Website Root														|	c:\\inetpub\\wwwroot\\habitathome.dev.local
| XConnect Root 		| Location of IIS xConnect Site Root 													| C:\\Inetpub\\wwwroot\\habitathome_xconnect.dev.local\\
| Instance Url 			| Url of site 																			| https://habitathome.dev.local/
| BuildConfiguration 	| 'Debug/Release' point to NuGet, 'Local' copies DLLs from an existing installation 	| Debug
| DeploymentTarget  	| Local/OnPrem/Azure - see below for details 											| Local
| DeployFolder 			| Used for WDP generation and Azure deployments 										| C:\\deploy
| Version 				| Version of Sitecore being targeted. Must match official 3-digit version 				| 9.1.1
| Topology 				| Target topology for WDP creation and Azure deployment. Values are single or scaled 	| single
| CDN 					| Content Delivery Network enabled (true/false). Used only when deploying to Azure 		| false

**DeploymentTarget:**
- **Local**: Set to deploy the site to the local Sitecore instance
	- Ignored when calling Build-WDP target where OnPrem is assumed
- **OnPrem**: Used when generating a WDP. Targets the WDP for OnPrem transforms (a.k.a. not Azure)
- **Azure**: Used when deploying to Azure or generating WDPs which target Azure PaaS deployments
</details>

#### Deploying HabitatHome Locally
Prior to attempting the demo installation, ensure you have a working **Sitecore XP 9.1** instance. Detailed installation instructions can be found at [doc.sitecore.com](https://dev.sitecore.net/Downloads/Sitecore_Experience_Platform/91/Sitecore_Experience_Platform_91_Initial_Release.aspx).

[Jump to local deployment instructions](#localInstallation) 
#### Generating WebDeploy Package (WDP)
It is now possible to build a custom Web Deploy Package of the Habitat Home project (including xConnect components). The generated WDP can then be installed using SIF for OnPrem or ARM for Azure. 

The [Sitecore.HabitatHome.Utilities repo](https://github.com/sitecore/sitecore.habitathome.utilities) now includes an `install-habitathome.ps1` script and relevant SIF tasks to download and install Habitat Home as a package (or of course use your own generated package).

[Jump to WDP build instructions](#wdp)

#### Azure (PaaS) Deployment
It is now possible to build, package and depoy to Azure (PaaS) with one command. The script will build Habitat Home, package it for Azure, download the required Sitecore assets and upload them to a (specified) Azure Storage Account.
[Jump to Azure deployment instructions](#azure)


<a name="localInstallation"></a>
## Local Build / Deployment
<details>
#### The hostname habitathome.dev.local is used in the SXA Hostname (Site Grouping). 

If you do not use habitathome.dev.local you will need to modify the Host Name in 
`/sitecore/content/Habitat SXA Sites/Habitat Home/Settings/Site Grouping/Habitat Home` after successfully deploying the site.
The Habitat Home site will not respond / render correctly until this value is modified. 

If you do **not want to use the default settings**, you need to adjust the appropriate values in `cake-config.json` file based on the values described earlier.

The cake script will automatically create a publishSettings.targets.user file with the value of the InstanceUrl specified in the cake-config.json file.

## Installation:

All installation instructions assume using **PowerShell 5.1** in _**administrative**_ mode.


### 1. Deploy Sitecore.HabitatHome.Platform

#### *IMPORTANT: Publish Sitecore Instance after installing all required and optional modules BEFORE trying to deploy Habitat Home*

From the root of the solution

- Run **`.\build.ps1`**
	- Notes:
		- If the deployment fails at `Sync-Unicorn` or `Deploy-EXM-Campaigns` step, evaluate and fix the error (if any) and then run `.\build.ps1 -Target "Post-Deploy"`.


### 2. Validating deployment

1. Browse to https://habitathome.dev.local (or whatever hostname you selected)
	1. You should see the Habitat Home landing page with a full-width carousel
	2. If you do not see the full-width carousel and instead see the initial Sitecore default landing page, ensure that your Host Name was configured correctly in `/sitecore/content/Habitat SXA Sites/Habitat Home/Settings/Site Grouping/Habitat Home` and that the site has published successfully 


## Additional Settings
### 1. Disable Unicorn Serialization
When Unicorn is active, the Content Editor will display warnings that certain items are controlled by Unicorn. If you wish to disable Unicorn serialization, open the Web.config file in your webroot and update the following appSetting

    <add key="unicorn:define" value="Off"/>
This appSetting is `On` by default. Setting it to `Off` ensures that none of the Unicorn serialization configuration files are loaded.
</details>


<a name="wdp"></a>
## Generating Web Deploy Package (WDP)
<details>
CakeBuild (```build.cake```) contains tasks to build and package Habitat Home for use either OnPrem or in Azure PaaS. The settings in the cake-config.json file drive the packaging behaviour.

The process of creating a WDP of Habitat Home and its xConnect project is quite simple. The build process requires dev.sitecore.com credentials since it has a dependency on Sitecore Azure Toolkit and it will download and extract it automatically.

A few settings are important in the `cake-config.json` file:
- **DeploymentTarget**: Set to **OnPrem** for deploying locally or in Azure IaaS. Set to **Azure** for PaaS deployments
- **DeployFolder**: Temporary location where work will be performed. Defaults to c:\deploy	
- **Version**:	Version of Sitecore being targeted. Must match official 3-digit version	9.1.1 and of course the Habitat Home target version you're working with.
- **Topology**: Values are **single** (XP0/XPSingle) or **scaled** (XP1/XPScaled)
- **CDN**: Configure the WDP to support Content Delivery Network (**true/false**). Used only when deploying to Azure.

Once you got the settings just right, you can call the cake build script and pass it in the correct target and your dev.sitecore.net credentials (either at command line or as a user environment variable).

```.\build.ps1 -Target Build-WDP -ScriptArgs --DEV_SITECORE_USERNAME=your_e-mail, --DEV_SITECORE_PASSWORD=YourPassword```
> if you've set DEV_SITECORE_USERNAME and DEV_SITECORE_PASSWORD as environment variables you can omit them from the command line.

Once the process completes, you should have WDPs for HabitatHome as well as xConnect in `<DeployFolder>\9.1.1\XPSingle\assets\HabitatHome\WDPWorkFolder\WDP` and `<DeployFolder>\9.1.1\XPSingle\assets\xConnect\WDPWorkFolder\WDP`

You can then install these WDPs using SIF. An [example script](https://github.com/Sitecore/Sitecore.HabitatHome.Utilities/blob/master/XP/install/install-habitathome.ps1) already exists in the [HabitatHome.Utilities](https://github.com/Sitecore/Sitecore.HabitatHome.Utilities/tree/master) repo
</details>

<a name="azure"></a>
## Azure (PaaS) Deployment
<details>
This is probably the most comprehensive script which makes getting your own version. Once your variables are set and your Azure Service Principal is created, you can deploy Habitat Home to Azure PaaS in a single command.

### One-time step

### 1. Create an Azure Service Principal
> Only needs to be created once

In order for the upload and deployment process to authenticate to your Azure tenant you will need to provide a Service principal using password-based authentication, and its related information.

This is best done from Azure's CLI: [Azure CLI Documentation](https://docs.microsoft.com/en-us/azure/cloud-shell/quickstart)

Run the following, replacing *ServicePrincipalName* and *PASSWORD* with your own values:

`az ad sp create-for-rbac --name ServicePrincipalName --password PASSWORD`

This will return the following:

```json
{
  "appId": "APP_ID",
  "displayName": "ServicePrincipalName",
  "name": "http://ServicePrincipalName",
  "password": ...,
  "tenant": "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX"
}
```

The `appId`, `password`, and `tenant` values will be required later so make sure to record them!

[Create a Service Principal Documentation](https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli?view=azure-cli-latest)

### 2. Setting up the parameters

In the Azure/XP or XPSingle folders there is an `azureuser-config.json.example` file. Make a copy of that file and remove the .example extension.

Set the following values in the `XP\azureuser-config.json` *(scaled topology)* or `XPSingle\azureuser-config.json` *(single topology)* in  based on your needs:

|Parameter                                  | Description
|-------------------------------------------|---------------------------------------------------------------------------------------------
| azureSubscriptionName                     | The name or id of the Azure subscription. Can be found under the "Subscriptions" Dashboard
| tenantId                                  | Also called a DirectoryId. Can be found in the "Azure Active Directoy" Dashboard under Manage -> Properties
| applicationId                             | appId of the Service Principal *(Created in previous step)*
| applicationPassword                       | the Service Principal password *(Created in previous step)*
| AzureDeploymentID                         | The Resource Group name in azure that Habitat will be deployed to. If the group does not exist it will be created.
| AzureRegion                               | The Geographic Azure Location of the Deployment [Azure Locations](https://azure.microsoft.com/en-us/global-infrastructure/locations/)
| XConnectCertfilePath                      | xConnect Certificate Path. **This will be auto generated if left blank.**
| XConnectCertificatePassword               | xConnect Certificate Password. Defaults to 'secret' if this and *XConnectCertfilePath* is left blank.
| SitecoreLoginAdminPassword                | Sitecore Administrator Password (8 Character Minimum)
| SitecoreLicenseXMLPath                    | Sitecore license.xml Path
| SqlServerLoginAdminAccount                | SQL Server Administrator Username (SA is not a valid admin name for Azure SQL)
| SqlServerLoginAdminPassword               | SQL Server Administrator Password
| containerName                             | name of the Azure container. This will be auto generated if left blank. Defaults to 'hh-toolkit'
| storageAccountName                        | name of the Azure Storage Account. This will be auto generated if left blank. By default *AzureDeploymentID + Random Number*
| ArmTemplateUrl                            | Azure SAS URL of the azuredeploy.json. This will be auto generated if left blank.
| templatelinkAccessToken                   | Azure SAS token for the container. This will be auto generated if left blank.

### 3. Deploy

Once you got the settings just right, you can call the cake build script and pass it in the correct target and your dev.sitecore.net credentials (either at command line or as a user environment variable).

```.\build.ps1 -Target Default-Azure -ScriptArgs --DEV_SITECORE_USERNAME=your_e-mail, --DEV_SITECORE_PASSWORD=YourPassword```
> if you've set DEV_SITECORE_USERNAME and DEV_SITECORE_PASSWORD as environment variables you can omit them from the command line.

Once the process completes, you should have a single or scaled deployment to Azure (AzureDeploymentID is the resource group name).

#### Azure Deployment - Process Explained

The Azure deployment scripts will perform the following tasks, in one single command:
1. Compile Habitat Home solution
1. Generate a WDP based on the topology and DeploymentTarget
1. Download all Sitecore assets and optional modules required for a depoyment based on topology (stores them in *DeployFolder*)
2. Downloads relevant ARM templates from [GitHub](https://github.com/Sitecore/Sitecore-Azure-Quickstart-Templates), applies necessary transformations and uploads them to your specified storage account.
3. Uploads all assets (Sitecore, modules and Habitat Home) to storage account (only if missing or newer)
4. Creates the Deployment
5. Executes postSteps

# Scaling down - IMPORTANT
During the deployment process, certain infrastructure pieces are scaled up to ensure an efficient deployment. These are generally more costly and can be scaled down since they don't need to be so large.

A [scale-down](https://github.com/Sitecore/Sitecore.HabitatHome.Platform/blob/master/Azure/HelperScripts/Azure-Scaledown.ps1) script is available. It currently only supports the App Service plans but there is a commented out section for scaling down the databases. 

Alternatively this can be done directly from the Azure Portal. 

### Assets.json
Only alter these attributes as instructed, the build/deploy process relies on several of these parameters and are maintained by the repository maintainers.

|Parameter                                  | Description
|-------------------------------------------|---------------------------------------------------------------------------------------------
| id                                        | short-name
| name                                      | long-name
| isGroup                                   | denotes if the asset is a collection of modules.
| fileName                                  | Exact default filename
| url                                       | download link
| extract                                   | instructs the script that a zip file should be extracted. It will extract it to deploy/assets/name of asset
| isWdp                                     | asset is already a scwdp (this is false for "Sitecore Experience Platform" as the file decalred in the asset.json is a .zip)
| convertToWdp                              | asset needs to be converted to an scwdp
| uploadToAzure                             | asset should be uploaded to Azure. if isGroup is true, will upload the assetes in the modules parameter.
| install                                   | asset should be downloaded and installed (the only asset this currently functions with is DEF)
| source                                    | download source, sitecore or github have specific requirements for downloading (e.g. credentials )
| modules                                   | list modules for groups
</details>

# Contribute or Issues
Please post any issues on Slack Community [#habitathome](https://sitecorechat.slack.com/messages/habitathome/) channel or create an issue on [GitHub](https://github.com/Sitecore/Sitecore.HabitatHome.Platform/issues). Contributions are always welcome!
