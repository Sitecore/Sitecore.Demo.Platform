# Introduction 

This Section is dedicated to the deployment of the Habitat Home Demo project onto Azure PaaS.

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

### 2. Customize install

The following is a list of default values/assumptions for install locations

**Project Folder**		`c:\projects\sitecore.habitathome.content\`  
**Deploy Folder**		`c:\Deploy`  

if these values are not correct you will need to edit the cake-config.json in the `\Azure PaaS` folder.  

### 3. Deploy Sitecore.HabitatHome.Content

From the `\Azure PaaS` folder

- Run **`.\build.ps1`** 

Optional Parameters:
|Parameter                                  | Description
|-------------------------------------------|---------------------------------------------------------------------------------------------
| Verbosity                                 | Specifies the amount of information to be displayed
| ShowDescription                           | Shows description about tasks.
| DryRun                                    | Performs a dry run.
| SkipToolPackageRestore                    | Skips restoring of packages.
| Target                                    | Build Target **(see below)**

**Target**
|Value                                  | Description
|-------------------------------------------|---------------------------------------------------------------------------------------------
| Default                                    | Same affect as no target defined, will build and deploy to Azure.
| Build                                      | Create build output only, will **not** upload or deploy to Zzure
| Azure-Upload                               | Performs only the upload to Azure portion of the process
| Azure-Deploy                               | Performs only the Deployment portion of the process

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

### 4. Validating deployment

##### The hostname habitathome.dev.local is used in the SXA Hostname (Site Grouping). 

If you do not use habitathome.dev.local you will need to modify the Host Name in 
`/sitecore/content/Habitat Sites/Habitat Home/Settings/Site Grouping/Habitat Home` after successfully deploying the site.
The Habitat Home site will not respond / render correctly until this value is modified. 

# Contribute or Issues
Please post any issues on Slack Community [#habitathome](https://sitecorechat.slack.com/messages/habitathome/) channel or create an issue on [GitHub](https://github.com/Sitecore/Sitecore.HabitatHome.Content/issues). Contributions are always welcome!