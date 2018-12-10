# Introduction 
HabitatHome  Demo and the tools and processes in it is a Sitecore&reg; solution example built using Sitecore Experience Accelerator&trade; (SXA) on Sitecore Experience Platform&trade; (XP)  following the Helix architecture principles.


# Important Notice

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

# Getting Started

### Prerequisites

The latest Habitat Home demo is built to support **[Sitecore Experience Platform 9.1](https://dev.sitecore.net/Downloads/Sitecore_Experience_Platform/91/Sitecore_Experience_Platform_91_Initial_Release.aspx)** using **[Sitecore Experience Accelerator (SXA) 1.8](https://dev.sitecore.net/Downloads/Sitecore_Experience_Accelerator/18/Sitecore_Experience_Accelerator_180.aspx)**.

***In order to follow the build instructions in the README, you need to have [MSBuild Tools for Visual Studio 2017](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2017) installed.***

### Optional Modules
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

### SSL Only
The demo is configured for **HTTPS/SSL**. Please ensure that you create an HTTPS binding with appropriate self-signed certificates.

### Clone this repository

#### Setting Git for Long Paths

- Before cloning, you need to configure git to allow long paths, which is not the default.

`git config --system core.longpaths true`

Clone the Sitecore.HabitatHome.Platform repository locally - defaults are configured for **C:\Projects\Sitecore.HabitatHome.Platform**. 


- Clone 
-- **https**:	`git clone https://github.com/Sitecore/Sitecore.HabitatHome.Platform.git` 
-- **ssh**:		`git clone git@github.com:Sitecore/Sitecore.HabitatHome.Platform.git`


#### Parameters - explained

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
| Version 				| Version of Sitecore being targeted. Must match official 3-digit version 				| 9.1.0
| Topology 				| Target topology for WDP creation and Azure deployment. Values are single or scaled 	| single
| CDN 					| Content Delivery Network enabled (true/false). Used only when deploying to Azure 		| false

**DeploymentTarget:**
- Local: Set to deploy the site to the local Sitecore instance
	- Ignored when calling Build-WDP target where OnPrem is assumed
- OnPrem: Used when generating a WDP. Targets the WDP for OnPrem transforms (a.k.a. not Azure)
- Azure: Used when deploying to Azure or generating WDPs which target Azure PaaS deployments


#### Deploying HabitatHome Locally
Prior to attempting the demo installation, ensure you have a working **Sitecore XP 9.1** instance. Detailed installation instructions can be found at [doc.sitecore.com](https://dev.sitecore.net/Downloads/Sitecore_Experience_Platform/91/Sitecore_Experience_Platform_91_Initial_Release.aspx).

[Jump to local deployment instructions](#localInstallation) 
#### Generating WebDeploy Package (WDP)
It is now possible to build a custom Web Deploy Package of the Habitat Home project (including xConnect components). The generated WDP can then be installed using SIF for OnPrem or ARM for Azure. 

The [Sitecore.HabitatHome.Utilities repo](https://github.com/sitecore/sitecore.habitathome.utilities) now includes an `install-habitathome.ps1` script and relevant SIF tasks to download and install Habitat Home as a package (or of course use your own generated package).

[Jump to WDP build instructions](#wdp)

#### Azure (PaaS) Deployment
It is now possible to build, package and depoy to Azure (PaaS) with one command. The script will build Habitat Home, package it for Azure, download the required Sitecore assets and upload them to a (specified) Azure Storage Account.
[Jump to Azure deployment instructions](#wdp)


<a name="localInstallation"></a>
## Local Build / Deployment

#### The hostname habitathome.dev.local is used in the SXA Hostname (Site Grouping). 

If you do not use habitathome.dev.local you will need to modify the Host Name in 
`/sitecore/content/Habitat Sites/Habitat Home/Settings/Site Grouping/Habitat Home` after successfully deploying the site.
The Habitat Home site will not respond / render correctly until this value is modified. 

If you do **not want to use the default settings**, you need to adjust the appropriate values in `cake-config.json` file:

- **WebsiteRoot**
- **XConnectRoot**
- **ProjectFolder**
- **InstanceUrl**

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
	2. If you do not see the full-width carousel and instead see the initial Sitecore default landing page, ensure that your Host Name was configured correctly in `/sitecore/content/Habitat Sites/Habitat Home/Settings/Site Grouping/Habitat Home` and that the site has published successfully 


## Additional Settings
### 1. Disable Unicorn Serialization
When Unicorn is active, the Content Editor will display warnings that certain items are controlled by Unicorn. If you wish to disable Unicorn serialization, open the Web.config file in your webroot and update the following appSetting

    <add key="unicorn:define" value="Off"/>
This appSetting is `On` by default. Setting it to `Off` ensures that none of the Unicorn serialization configuration files are loaded.



<a name="wdp"></a>
## Generating Web Deploy Package (WDP)

CakeBuild (```build.cake```) contains tasks to build and package Habitat Home for use either OnPrem or in Azure PaaS. The settings in the cake-config.json file drive the packaging behaviour.


# Contribute or Issues
Please post any issues on Slack Community [#habitathome](https://sitecorechat.slack.com/messages/habitathome/) channel or create an issue on [GitHub](https://github.com/Sitecore/Sitecore.HabitatHome.Platform/issues). Contributions are always welcome!