# Using Lighthouse Demo

## Clone this repository

Clone the Sitecore.Demo.Platform repository locally - defaults are configured for **C:\Projects\Sitecore.Demo.Platform**.

* **https**: `git clone https://github.com/Sitecore/Sitecore.Demo.Platform.git`
* **ssh**: `git clone git@github.com:Sitecore/Sitecore.Demo.Platform.git`

## Prerequisites

* Windows 1809 or higher. Version 1909 is preferred.
* At least 16 Gb of memory. 32 Gb or more is preferred.
* A valid Sitecore 10 license file located at `C:\license\license.xml`
* The latest [Docker Desktop](https://docs.docker.com/docker-for-windows/install/).
* From the Docker Desktop menu, you can toggle which daemon (Linux or Windows) the Docker CLI talks to. Select "Switch to Windows containers..." to use Windows containers.
* Pre-built [docker-images](https://github.com/Sitecore/docker-images/blob/master/README.md) stored locally or in your own Docker registry.
* Ensure the SXA NPM registry is configured:
  * CMD: `npm config set @sxa:registry https://sitecore.myget.org/F/sc-npm-packages/npm/`
  * PowerShell: ``npm config set `@sxa:registry https://sitecore.myget.org/F/sc-npm-packages/npm/``
* [MSBuild Tools for Visual Studio 2019](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=BuildTools&rel=16).
* [https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows?view=azure-cli-latest]("az" PowerShell module).

## Preparing your environment

1. Modify your `.env` file:
   * `REGISTRY`: Set your registry url (The trailing slash (/) is very important).
   * `SQL_SA_PASSWORD`: Set the SA password to the one configured in your base image.
2. Open an elevated (as administrator) PowerShell session.
3. Create certificates and initialize the environment file:
   * `.\init.ps1 -InitEnv -LicenseXmlPath C:\license\license.xml -AdminPassword b`
   * You can change the admin password.
4. Login to your docker registry:
   * For Azure ACR:
     * `az login`
     * `az acr login --name <registryname>`
5. Pull the latest base images:
   * `docker-compose pull`
   * this will pull all of the necessary base images to spin up your Sitecore environment. It will take quite some time if this is the first time you execute it.

## Building Docker images

1. Open an elevated (as administrator) PowerShell session.
2. Build your Docker images:
   * `docker-compose build -m 8G`
   * This command will use up to 8 Gb of memory to build the Docker images. Adjust the number based on your available free memory.

## Starting the demo containers

1. Open an elevated (as administrator) PowerShell session.
2. Start the demo containers:
   * `docker-compose up -d`
   * The Sitecore instance will be up and available within minutes, but not fully working until the init container jobs are completed. The init container runs scripts that:
     * Publish the master database to the web database using Sitecore Publishing Service.
     * Warmup CM and CD pages for a fast first load.
     * Deploy Sitecore marketing definitions.
     * Rebuild Sitecore and SXA indices.
     * Generate analytics data using Sitecore Experience Generator.
   * Loading the Sitecore instance before the completion of the init container may cause:
     * Marketing Automation plans may not work as Sitecore marketing definitions are not deployed.
     * Some Content Editor features and other admin pages relying on search indices may not work.
     * The search page and search based components may not work on the CD.
3. Check the progress of the initialization by viewing the init container's logs:
   * `docker-compose logs -f init`
4. Wait about 30 minutes until the init container logs can read `No jobs are running. Monitoring stopped.`.

## Validating deployment

1. Browse to [https://cd.lighthouse.localhost](https://cd.lighthouse.localhost)
   1. You should see the Lighthouse landing page with a full-width carousel.
   2. If you do not see the full-width carousel and instead see the initial Sitecore default landing page, ensure that all the "init" container jobs are completed by checking its logs.
2. Browse to [https://cm.lighthouse.localhost/sitecore](https://cm.lighthouse.localhost/sitecore)
   1. You should be able to login with the "admin" user and the password provided while running the `init.ps1` script.

## Development cycle

After changes to the code:

1. Open an elevated (as administrator) PowerShell session.
2. `.\build.ps1 -DeploymentTarget Docker`

## Starting over

If you want to reset all of your changes and get a fresh intsance:

1. Run `docker-compose down`
2. Run `.\CleanDockerData.ps1`
3. At this point you can start again with `docker-compose up -d` to have a fresh installation of Sitecore with no files/items deployed!

## Troubleshooting deployment

### unauthorized: authentication required

When running `docker-compose up -d`, you get the following error:

```text
ERROR: Get https://<registryname>/<someimage>/manifests/<someimage>: unauthorized: authentication required
```

This indicates you are not logged in your registry. Run `az acr login --name <registryname>` (or the equivalent `docker login`) and retry.

## Additional Settings

### Cake build settings explained

The following is a list of default values / assumptions for settings in `cake-config-containers.json`.

| Parameter | Description | Default Value |
|-|-|-|
| PublishWebFolder | Location of the CM build output in the build containers. | `"c:\\out\\demo-standalone"` |
| PublishWebFolderCD | Location of the CD build output in the build containers. | `"c:\\out\\demo-cd"` |
| PublishxConnectFolder | Location of the xConnect build output in the build containers. | `"c:\\out\\demo-xconnect"` |
| PublishxConnectIndexWorkerFolder | Location of the xConnect index worker build output in the build containers. | `"c:\\out\\demo-indexworker"` |
| SolutionName | Name of the Visual Studio solution file. | `"Sitecore.Demo.Platform.sln"` |
| ProjectFolder | Location of the Visual Studio solution file in the build containers. | `"C:"` |
| UnicornSerializationFolder | Location of the Unicorn serialized files in the mssql build container. | `"C:\\items"` |
| BuildConfiguration | Can be `Debug` or `Release`. | `"Debug"` |
| BuildToolVersions | Version of the Microsoft Visual Studio build tools that are used to build. | `"VS2019"` |
| RunCleanBuilds | Whether to clean the build output for every build. | `false` |
| MessageStatisticsApiKey | API key for the message statistics. | `"97CC4FC13A814081BF6961A3E2128C5B"` |
| MarketingDefinitionsApiKey | API key for the marketing definitions. | `"DF7D20E837254C6FBFA2B854C295CB61"` |
| DeployExmTimeout | The timeout, in seconds, to wait for the deployment of EXM. | `60` |
| PublishTempFolder | Location of the temporary build publishing folder in the build containers. | `"c:\\publishTemp"` |
| version | Version of Sitecore being targeted. Must match official 3-digit version. | `"10.0.0"` |
| IsContentHubEnabled | Whether Content Hub should be enabled. | `"false"` |
| SitecoreAzureToolkitPath | The location of the Sitecore Azure Toolkit files on the host computer. | `"c:\\sat"` |

### Disable Unicorn Serialization

When Unicorn is active, the Content Editor will display warnings that certain items are controlled by Unicorn. If you wish to disable Unicorn serialization:

1. Open the `docker-compose.override.yml` file in your repository clone.
2. Find all occurrences of the `SITECORE_APPSETTINGS_UNICORN:DEFINE` environment variable.
3. Update all occurences to `SITECORE_APPSETTINGS_UNICORN:DEFINE: Disabled`
4. Save the file.
5. Run `docker-compose up -d` again to update the containers.

This setting is set to `Enabled` by default. Setting it to `Disabled` ensures that none of the Unicorn serialization configuration files are loaded.
