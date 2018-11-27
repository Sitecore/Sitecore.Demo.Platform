# Habitat Home Sitecore XP0 Single Environment Azure

## Instructions

Please follow the instructions on the [Azure Readme](../Readme.md)

## Resources provisioned:

  * Habitat Home 
  * Azure SQL databases : core, master, web, reporting, pools, tasks, forms, refdata, smm, shard0, shard1, ma
  * Sitecore roles: Content Delivery, Content Management, Processing, Reporting as a single WebApp instance
    * Hosting plans: single hosting plan
    * Preconfigured Web Application, based on the provided WebDeploy package
  * XConnect services: Search, Collection, Reference data, Marketing Automation, Marketing Automation Reporting as a single WebApp instance
    * Hosting plans: single hosting plan
    * Preconfigured Web Application, based on the provided WebDeploy package
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
| AzureDeploymentID                         | Resource group name.
| AzureRegion                               | The geographical region of the current deployment.
| SitecoreLoginAdminPassword                | The new password for the Sitecore **admin** account.
| SqlServerLoginAdminAccount                | The name of the administrator account for Azure SQL server that will be created. SA is not a valid login
| SqlServerLoginAdminPassword               | The password for the administrator account for Azure SQL server.
| XConnectCertfilePath                      | A Base64-encoded blob of the authentication certificate in PKCS #12 format.
| XConnectCertificatePassword               | A password to the authentication certificate.


> **Note:**
> * The **searchServiceLocation** parameter can be added to the `azuredeploy.parameters.json`
> to specify geographical region to deploy Azure Search Service. Default value is the resource
> group location.
> * The **applicationInsightsLocation** parameter can be added to the`azuredeploy.parameters.json`
> to specify geographical region to deploy Application Insights. Default value is **East US**.
> * The **allowInvalidClientCertificates** has been parameter added to the `azuredeploy.parameters.json` to allow self signed certificates