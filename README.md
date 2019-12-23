# Introduction

HabitatHome  Demo and the tools and processes in it is a Sitecore&reg; solution example built using Sitecore Experience Accelerator&trade; (SXA) on Sitecore Experience Platform&trade; (XP)  following the Helix architecture principles.

## Important Notice

### Is Habitat Home a starter kit or template solution

No. You should not clone this repository for the purposes of starting a new Sitecore project. There are other community solutions which can be used as a starter for Helix-based Sitecore implementations. Habitat Home is intended as a **demo site demonstrating the full Sitecore platform capabilities and development best practices**.

### Is Habitat Home supported by Sitecore

Sitecore maintains the Habitat Home example, but Habitat Home code is not supported by Sitecore Product Support Services. Please do not submit support tickets regarding Habitat.

### How can I get help with Habitat Home

For usage questions regarding Habitat Home, installation or code, please use [Sitecore Stackexchange](https://sitecore.stackexchange.com/) or [#habitathome](https://sitecorechat.slack.com/messages/CASEB5M38) on [Sitecore Community Slack](https://www.akshaysura.com/2015/10/27/how-to-join-sitecore-slack-community-chat/).

You can use GitHub to submit [bug reports](https://github.com/Sitecore/Sitecore.HabitatHome.Platform/issues/new?template=bug_report.md) or [feature requests](https://github.com/Sitecore/Sitecore.HabitatHome.Platform/issues/new?template=feature_request.md) for Habitat Home. Please do not submit usage questions via GitHub.

### License

Please read the LICENSE carefully prior to using the code in this repository.

### Support

The code, samples and/or solutions provided in this repository are ***unsupported by Sitecore PSS***. Support is provided on a best-effort basis via GitHub issues or Slack #habitathome (see end of README for additional information).

It is assumed that you already have a working instance of Sitecore XP and all prerequisites prior to installing the demo. Support for **product installation** issues should be directed to relevant Community channels or through regular Sitecore support channels.

### Warranty

The code, samples and/or solutions provided in this repository are for example purposes only and **without warranty (expressed or implied)**. The code has not been extensively tested and is not guaranteed to be bug free.

## Getting Started

### Prerequisites

The latest Habitat Home demo is built to support **[Sitecore Experience Platform 9.3](https://dev.sitecore.net/Downloads/Sitecore_Experience_Platform/93/Sitecore_Experience_Platform_93_Initial_Release.aspx)** using **[Sitecore Experience Accelerator (SXA) 9.3](https://dev.sitecore.net/Downloads/Sitecore_Experience_Accelerator/9x/Sitecore_Experience_Accelerator_930.aspx)**.

***In order to follow the build instructions in the README, you need to have [MSBuild Tools for Visual Studio 2019](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=BuildTools&rel=16) installed.***

### Optional Modules

The following optional modules enable synchronization with Dynamics CRM or Salesforce CRM (relevant account required)

* Base Data Exchange Framework modules
  * [Data Exchange Framework v2.1.0](https://dev.sitecore.net/~/media/C10E96CD1EAC46C49C957D5C3445BFB2.ashx) on [dev.sitecore.com](https://dev.sitecore.net/Downloads/Data_Exchange_Framework/2x/Data_Exchange_Framework_210.aspx) as well as relevant Providers on same page
    * [Sitecore Provider for Data Exchange Framework 2.1.0](https://dev.sitecore.net/~/media/D80B9AE68C71473E895608806A764332.ashx)
    * [SQL Provider for Data Exchange Framework 2.1.0](https://dev.sitecore.net/~/media/52203CC3ADCD4668AF0D7568EF65A7BD.ashx)
    * [xConnect Provider for Data Exchange Framework 2.1.0](https://dev.sitecore.net/~/media/678E59D0B92C4F20B0025730958A15A0.ashx)
    * Dynamics-specific modules
      * Data Exchange Framework 2.1.0 Dynamics Connectors on [dev.sitecore.com](https://dev.sitecore.net/Downloads/Dynamics_CRM_Connect/2x/Sitecore_Connect_for_Microsoft_Dynamics_365_for_Sales_210.aspx)
      * [Microsoft Dynamics 365 for Sales Provider for Data Exchange Framework 2.1.0](https://dev.sitecore.net/~/media/5F22998E037C4F0E9A951D811F67A424.ashx)
      * [Sitecore Connect for Microsoft Dynamics 365 for Sales 2.1.0](https://dev.sitecore.net/~/media/E1A8A968BEE347BA81255ADB132FD480.ashx)
    * Salesforce-specific modules
      * Sitecore Connect for Salesforce CRM 2.1.0 on [dev.sitecore.com](https://dev.sitecore.net/Downloads/Salesforce_Connect/2x/Sitecore_Connect_for_Salesforce_CRM_210.aspx)
      * [Salesforce CRM Provider for Data Exchange Framework 2.1.0](https://dev.sitecore.net/Downloads/Salesforce_Connect/2x/Sitecore_Connect_for_Salesforce_CRM_210.aspx#)
      * [Sitecore Connect for Salesforce CRM 2.1.0](https://dev.sitecore.net/Downloads/Salesforce_Connect/2x/Sitecore_Connect_for_Salesforce_CRM_210.aspx#)

### SSL Only

The demo is configured for **HTTPS/SSL**. Please ensure that you create an HTTPS binding with appropriate self-signed certificates.

### Clone this repository

Clone the Sitecore.HabitatHome.Platform repository locally - defaults are configured for **C:\Projects\Sitecore.HabitatHome.Platform**.

* **https**: `git clone https://github.com/Sitecore/Sitecore.HabitatHome.Platform.git`
* **ssh**: `git clone git@github.com:Sitecore/Sitecore.HabitatHome.Platform.git`

### Parameters - explained

The following is a list of default values / assumptions for settings ( `cake-config.json` )

|Parameter                                  | Description | Default Value
|-------------------------------------------|----------------------------------|-------------------------------------------------------
| ProjectFolder         | Location of Sitecore. HabitatHome. Platform project | c:\projects\Sitecore. HabitatHome. Platform |
| Website Root | Location of IIS Website Root | c:\\inetpub\\wwwroot\\habitathome.dev.local
| XConnect Root | Location of IIS xConnect Site Root | C:\\Inetpub\\wwwroot\\habitathome_xconnect.dev.local\\
| Instance Url | Url of site | https://habitathome.dev.local/
| BuildConfiguration | 'Debug/Release' point to NuGet, 'Local' copies DLLs from an existing installation | Debug
| DeployFolder | Used for WDP generation and Azure deployments | C:\\deploy
| Version | Version of Sitecore being targeted. Must match official 3-digit version | 9.3.0

### Deploying HabitatHome Locally

Prior to attempting the demo installation, ensure you have a working **Sitecore XP 9.3** instance. Detailed installation instructions can be found at [doc.sitecore.com](https://dev.sitecore.net/Downloads/Sitecore_Experience_Platform/93/Sitecore_Experience_Platform_93_Initial_Release.aspx).

## Local Build / Docker Deployment 

### Prerequisites

1. Windows 1809 or higher, but prefer version 1909  
2. The latest Docker Desktop **[Docker Desktop](https://docs.docker.com/docker-for-windows/install/)** 
3. From the Docker Desktop menu, you can toggle which daemon (Linux or Windows) the Docker CLI talks to. Select Switch to Windows containers to use `Windows containers`
4. Pre-build [docker-images](https://github.com/Sitecore/docker-images/blob/master/README.md) local or taken from your own `ACR` 

## Starting the demo

1. Run `docker-compose up -d`
2. Browse to [http://localhost:44001](http://localhost:44001)
3. .\build.ps1 -DeploymentTarget Docker

## Local Build / Deployment 

### The hostname habitathome.dev.local is used in the SXA Hostname (Site Grouping)

If you do not use habitathome.dev.local you will need to modify the Host Name in
`/sitecore/content/Habitat SXA Sites/Habitat Home/Settings/Site Grouping/Habitat Home` after successfully deploying the site.
The Habitat Home site will not respond / render correctly until this value is modified.

If you do **not want to use the default settings**, you need to adjust the appropriate values in `cake-config.json` file based on the values described earlier.

The cake script will automatically create a publishSettings.targets.user file with the value of the InstanceUrl specified in the cake-config.json file.

### Installation

All installation instructions assume using **PowerShell 5.1** in _**administrative**_ mode.

#### 1. Deploy Sitecore. HabitatHome. Platform

**IMPORTANT**: Publish Sitecore Instance after installing all required and optional modules BEFORE trying to deploy Habitat Home*

From the root of the solution, run `.\build.ps1`

Note: If the deployment fails at `Sync-Unicorn` or `Deploy-EXM-Campaigns` step, evaluate and fix the error (if any) and then run `.\build.ps1 -Target "Post-Deploy"` .

#### 2. Validating deployment

* Browse to [https://habitathome.dev.local](https://habitathome.dev.local) (or whatever hostname you selected)
    1. You should see the Habitat Home landing page with a full-width carousel
    1. If you do not see the full-width carousel and instead see the initial Sitecore default landing page, ensure that your Host Name was configured correctly in `/sitecore/content/Habitat SXA Sites/Habitat Home/Settings/Site Grouping/Habitat Home` and that the site has published successfully

### Additional Settings

#### Disable Unicorn Serialization

When Unicorn is active, the Content Editor will display warnings that certain items are controlled by Unicorn. If you wish to disable Unicorn serialization, open the Web.config file in your webroot and update the following appSetting

    <add key="unicorn:define" value="Disabled"/>

This appSetting is `Enabled` by default. Setting it to `Disabled` ensures that none of the Unicorn serialization configuration files are loaded.

## Contribute or Issues

Please post any issues on Slack Community [#habitathome](https://sitecorechat.slack.com/messages/habitathome/) channel or create an issue on [GitHub](https://github.com/Sitecore/Sitecore.HabitatHome.Platform/issues). Contributions are always welcome!
