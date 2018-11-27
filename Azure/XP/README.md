# Habitat Home Sitecore XP1 Scaled Environment Azure

## Instructions

Please follow the instructions on the [Azure Readme](../Readme.md)

## Resources provisioned:

  * Habitat Home  
  * Azure SQL databases : core, master, web, reporting, pools, tasks, forms, exm.master, messaging, refdata, smm, shard0, shard1, ma
  * Azure Redis Cache for session state
  * Sitecore roles: Content Delivery, Content Management, Processing, Reporting
    * Hosting plans: one per role
    * Preconfigured Web Applications, based on the provided WebDeploy packages
  * XConnect services: Search, Collection, Reference data, Marketing Automation, Marketing Automation Reporting
    * Hosting Plans: XConnect Basic, XConnect Resource Intensive
    * Preconfigured Web Applications, based on the provided WebDeploy packages
  * Azure Search Service
  * Application Insights for diagnostics and monitoring
  * Modules
    * Sitecore Experience Accelerator 1.7.1 rev. 180604 for 9.0  
    * Sitecore PowerShell Extensions-4.7.2 for Sitecore 8  
    * Data Exchange Framework 2.0.1 rev. 180108  
    * Connect for Microsoft Dynamics 2.0.1 rev. 180108  
    * Dynamics Provider for Data Exchange Framework 2.0.1 rev. 180108  
    * Sitecore Provider for Data Exchange Framework 2.0.1 rev. 180108  
    * SQL Provider for Data Exchange Framework 2.0.1 rev. 180108  
    * xConnect Provider for Data Exchange Framework 2.0.1 rev. 180108  
    * Salesforce Connect Data Exchange Framework 2.0.1 rev. 180108  
    * Salesforce Provider for Data Exchange Framework 2.0.1 rev. 180108  

## azuredeploy.parameters

|Parameter                                  | Description
|-------------------------------------------|---------------------------------------------------------------------------------------------
| location                                  | The geographical region of the current deployment.
| sqlServerLogin                            | The name of the administrator account for Azure SQL server that will be created.
| sqlServerPassword                         | The password for the administrator account for Azure SQL server.
| sitecoreAdminPassword                     | The new password for the Sitecore **admin** account.
| repAuthenticationApiKey                   | A unique value (e.g. a GUID) that will be used as authentication key for communication between Content Management and the Reporting Web App. **Note: The minimal required length is 32 symbols**
| cmMsDeployPackageUrl                      | The HTTP(s) URL to a Sitecore XP Content Management Web Deploy package.
| cdMsDeployPackageUrl                      | The HTTP(s) URL to a Sitecore XP Content Delivery Web Deploy package.
| prcMsDeployPackageUrl                     | The HTTP(s) URL to a Sitecore XP Processing Web Deploy package.
| repMsDeployPackageUrl                     | The HTTP(s) URL to a Sitecore XP Reporting Web Deploy package.
| xcRefDataMsDeployPackageUrl               | The HTTP(s) URL to a XConnect Reference Data service Web Deploy package.
| xcCollectMsDeployPackageUrl               | The HTTP(s) URL to a XConnect Collection service Web Deploy package.
| xcSearchMsDeployPackageUrl                | The HTTP(s) URL to a XConnect Search service Web Deploy package.
| maOpsMsDeployPackageUrl                   | The HTTP(s) URL to a Marketing Automation service Web Deploy package.
| maRepMsDeployPackageUrl                   | The HTTP(s) URL to a Marketing Automation Reporting service Web Deploy package.
| authCertificateBlob                       | A Base64-encoded blob of the authentication certificate in PKCS #12 format.
| authCertificatePassword                   | A password to the authentication certificate.

> **Note:**
> * The **searchServiceLocation** parameter can be added to the `azuredeploy.parameters.json`
> to specify geographical region to deploy Azure Search Service. Default value is the resource
> group location.
> * The **applicationInsightsLocation** parameter can be added to the`azuredeploy.parameters.json`
> to specify geographical region to deploy Application Insights. Default value is **East US**.
> * The **allowInvalidClientCertificates** has been parameter added to the `azuredeploy.parameters.json` to allow self signed certificates