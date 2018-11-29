# Introduction 

This Section is dedicated to the deployment of the Habitat Home Demo project onto Azure.

# Important Notice
 
### Support

The code, samples and/or solutions provided in this repository are ***unsupported by Sitecore PSS***. Support is provided on a best-effort basis via GitHub issues or Slack #habitathome (see end of README for additional information).

It is assumed that you already have a working Azure tenant and associated account with necessary permissions. All normal Azure charges apply to this installation and should be carefully consdiered before deployment.

### Warranty

The code, samples and/or solutions provided in this repository are for example purposes only and **without warranty (expressed or implied)**. The code has not been extensively tested and is not guaranteed to be bug free.  

# Getting Started

## Prerequisites

- A working Azure tenant and associated account with necessary permissions.
- dev.sitecore.com credentials with permissions to download Sitecore installation files

### SSL Only
The demo is configured for **HTTPS/SSL**. Please ensure that you create an appropriate self-signed certificate

### Build / Deployment

In order to deploy the assets, you need either Visual Studio 2017 or MSBuild Tools for Visual Studio 2017.

## Installation:

All installation instructions assume using **PowerShell 5.1** in _**administrative**_ mode.

### 1. Clone this repository

#### Setting Git for Long Paths

- Before cloning, you need to configure git to allow long paths, which is not the default.

`git config --system core.longpaths true`

Clone the Sitecore.HabitatHome.Content repository locally - defaults are configured for `C:\Projects\Sitecore.HabitatHome.Content`. 


- Clone 
-- **https**:	`git clone https://github.com/Sitecore/Sitecore.HabitatHome.Content.git` 
-- **ssh**:		`git clone git@github.com:Sitecore/Sitecore.HabitatHome.Content.git`

### 2. Create an Azure Service Principal

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

### 3. Customize install

The following is a list of default values/assumptions for install locations

**Project Folder**		`c:\projects\sitecore.habitathome.platform`  
**Deploy Folder**		`c:\Deploy`  

if these values are not correct you will need to edit the cake-config.json in the `\Azure PaaS` folder. 
* Do not include a trailing `\` slash in your paths  
* Do not include any spaces in the Deploy folder directory path

#### Non-Interactive Installation

By default the deployment process will prompt you for necessary information. If you desire a non-interactive deployment then
you will need to modify the `azureuser-config.json` for your target topology.

|Parameter                                  | Description
|-------------------------------------------|---------------------------------------------------------------------------------------------
| azureSubscriptionName                     | The name or id of the Azure subscription. Can be found under the "Subscriptions" Dashboard
| tenantId                                  | Also called a DirectoryId. Can be found in the "Azure Active Directoy" Dashboard under Manage -> Properties
| applicationId                             | appId of the Service Principal
| applicationPassword                       | the Service Principal password
| sitecoreAccount                           | The username and password to a dev.sitecore.com accoutn with download permissions
| AzureDeploymentID                         | The Resource Group name in azure that Habitat will be deployed to. If the group does not exist it will be created.
| AzureRegion                               | The Geographic Azure Location of the Deployment [Azure Locations](https://azure.microsoft.com/en-us/global-infrastructure/locations/)
| XConnectCertfilePath                      | xConnect Certificate Path. This will be auto generated if left blank.
| XConnectCertificatePassword               | xConnect Certificate Password. If XConnectCertfilePath is left blank and a cert is generated for you then the value will default to the word "secret"
| SitecoreLoginAdminPassword                | Sitecore Administrator Password (8 Character Minimum)
| SitecoreLicenseXMLPath                    | Sitecore license.xml Path
| SqlServerLoginAdminAccount                | SQL Server Administrator Username (SA is not a valid admin name for Azure SQL)
| SqlServerLoginAdminPassword               | SQL Server Administrator Password
| containerName                             | name of the Azure container. This will be auto generated if left blank. By default hh-toolkit
| storageAccountName                        | name of the Azure Storage Account. This will be auto generated if left blank. By default *AzureDeploymentID + Random Number*
| ArmTemplateUrl                            | Azure SAS URL of the azuredeploy.json. This will be auto generated if left blank.
| templatelinkAccessToken                   | Azure SAS token for the container. This will be auto generated if left blank.

The Azure Container must have the following folder structure:

* Container  
	* arm-templates  
	* wdps

### 4. Deploy Sitecore.HabitatHome.Content

From the `\Azure` folder

- Run **`.\build.ps1`** 

Optional Parameters:

|Parameter                                  | Description
|-------------------------------------------|---------------------------------------------------------------------------------------------
| Verbosity                                 | Specifies the amount of information to be displayed
| ShowDescription                           | Shows description about tasks.
| DryRun                                    | Performs a dry run.
| SkipToolPackageRestore                    | Skips restoring of packages.
| Target                                    | Build Target **(see below)**
| SkipScUpload                              | Skip upload of sitecore packages. Will still upload Habitat Home and any other assets with the uploadToAzure and install flags set to true
| SkipPrerequisites                         | Skip the preparation steps like file downloads, asking users for Azure information and collection of credentials (Best used for testing purposes, or if the user has only run the project build, but information and prerequisites were already collected)

**Target**

|Value                                      | Description
|-------------------------------------------|---------------------------------------------------------------------------------------------
| Default                                   | Same affect as no target defined, will build and deploy to Azure
| Clean                                     | Clean all outputs and temporary folders, involved in the process of building and packaging the project
| Run-Prerequisites                         | Downloads Habitat Home prerequisites and captures user input on Azure and Sitecore account preferences
| Build                                     | Create build output only, will **not** upload or deploy to Azure
| Azure-Upload                              | Performs only the upload to Azure portion of the process
| Azure-Deploy                              | Performs only the Deployment portion of the process

##### Environment Preparation

This script will prompt you for information regarding your build and deployment

|Parameter                                  | Description
|-------------------------------------------|---------------------------------------------------------------------------------------------
| SitecoreDownloadUsername                  | dev.sitecore.com username
| SitecoreDownloadPassword                  | dev.sitecore.com password
| AzureDeploymentID                         | Resource group name.
| AzureRegion                               | The geographical region of the current deployment.
| SitecoreLoginAdminPassword                | The new password for the Sitecore **admin** account. (8 Character Minimum)
| SqlServerLoginAdminAccount                | The name of the administrator account for Azure SQL server that will be created. SA is not a valid login
| SqlServerLoginAdminPassword               | The password for the administrator account for Azure SQL server.
| XConnectCertfilePath                      | A Base64-encoded blob of the authentication certificate in PKCS #12 format.
| XConnectCertificatePassword               | A password to the authentication certificate.

## Contribute or Issues
Please post any issues on Slack Community [#habitathome](https://sitecorechat.slack.com/messages/habitathome/) channel or create an issue on [GitHub](https://github.com/Sitecore/Sitecore.HabitatHome.Content/issues). Contributions are always welcome!

## Sitecore Version Change
Alter the appropriate items in the assets.json file for the topology you are targeting. The only elements that should be changed are the filename and URL, 
> Do not alter the filename's default format.

Change the version parameter in the `Azure\cake-config.json` to the appropriate value.

### Assets.json
Only alter these attributes as instructed, the build/deploy process relies on several of these parameters.

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
