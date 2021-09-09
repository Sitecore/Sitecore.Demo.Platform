# Using Lighthouse Demo

## Table of Contents

- **[Clone this Repository](#clone-this-repository)**
- **[Prerequisites](#prerequisites)**
- **[Preparing Docker](#preparing-docker)**
- **[Preparing Your Environment](#preparing-your-environment)**
- **[Running the Demo](#running-the-demo)**
  - [Pulling the Docker Images](#pulling-the-docker-images)
  - [Starting the Demo Containers](#starting-the-demo-containers)
  - [Validating the Deployment](#validating-the-deployment)
  - [Optional - Enable Coveo for Sitecore](#Optional---Enable-Coveo-for-Sitecore)
- **[Stopping the Demo](#stopping-the-demo)**
- **[Starting Over](#starting-over)**
- **[Building the Demo](#building-the-demo)**
- **[Development Lifecycle](#development-lifecycle)**
  - [Local Build Prerequisites](#Local-Build-Prerequisites)
  - [After changes to the code](#After-changes-to-the-code)
- **[Troubleshooting](#troubleshooting)**

## Clone this Repository

[top](#table-of-contents)

Clone the Sitecore.Demo.Platform repository locally - defaults are configured for **`C:\Projects\Sitecore.Demo.Platform`**.

- **https**: `git clone https://github.com/Sitecore/Sitecore.Demo.Platform.git`
- **ssh**: `git clone git@github.com:Sitecore/Sitecore.Demo.Platform.git`

## Prerequisites

[top](#table-of-contents)

- Windows 1809 or higher. Version 1909 is preferred.
- At least 16 Gb of memory. 32 Gb or more is preferred.
- A valid Sitecore 10 license file located at `C:\license\license.xml`
- The latest [Docker Desktop](https://hub.docker.com/editions/community/docker-ce-desktop-windows/).

## Preparing Docker

[top](#table-of-contents)

1. Ensure you are running Windows containers:
   1. From the Docker Desktop taskbar icon contextual menu (right click), you can toggle which daemon (Linux or Windows) the Docker CLI talks to. Select "Switch to Windows containers..." to use Windows containers.
2. Ensure the Windows Docker engine experimental features are enabled (to allow the Linux smtp container to run at the same time as the Windows containers):
   1. From the Docker Desktop taskbar icon contextual menu (right click), choose "Settings".
   2. In the left tab group, navigate to the "Docker Engine" tab.
   3. In the JSON block, locate the `"experimental"` key.
      1. If you do not have an `"experimental"` key, add it after the existing ones. Ensure you add a comma (`,`) after the previous key/value pair.
   4. Ensure the value of the `"experimental"` key is set to `true`.
   5. At the end, the JSON block should have at least:

      ```json
      {
        "experimental": true
      }
      ```

   6. Optionally, you may want to also set DNS servers in the Docker engine configuration. See the [Issue downloading nodejs](#Issue%20downloading%20nodejs) known issue for details and instructions.
   7. Click the "Apply & Restart" button to restart your Windows Docker engine.

## Preparing Your Environment

[top](#table-of-contents)

1. Open an elevated (as administrator) PowerShell session.
2. Navigate to your repository clone folder:
   - `cd C:\Projects\Sitecore.Demo.Platform`
3. Create certificates and initialize the environment file:
   - `.\init.ps1 -InitEnv -LicenseXmlPath C:\license\license.xml -AdminPassword b`
   - You can change the admin password and the license.xml file path to match your needs.

## Running the Demo

### Pulling the Docker Images

[top](#table-of-contents)

1. Open an elevated (as administrator) PowerShell session.
2. Navigate to your repository clone folder:
   - `cd C:\Projects\Sitecore.Demo.Platform`
3. Pull the latest demo Docker images:
   - `docker-compose pull`

### Starting the Demo Containers

[top](#table-of-contents)

1. Open an elevated (as administrator) PowerShell session.
2. Navigate to your repository clone folder:
   - `cd C:\Projects\Sitecore.Demo.Platform`
3. Stop the IIS service: (_if applicable_)
   - `iisreset /stop`
   - This is required each time you want to use the demo as the Traefik container uses port (443), which is used by IIS.
4. Start the demo containers:
   - `docker-compose up -d`
   - This will pull all of the necessary images to spin up your Sitecore environment. It will take quite some time if this is the first time you execute it.
   - After pulling the images, the Sitecore instance will be up and available within minutes, but not fully working until the init container jobs are completed. The init container runs scripts that:
     - Publish the master database to the web database using Sitecore Publishing Service.
     - Warmup CM and CD pages for a fast first load.
     - Deploy Sitecore marketing definitions.
     - Rebuild Sitecore and SXA indexes.
     - Generate analytics data using Sitecore Experience Generator.
   - Loading the Sitecore instance before the completion of the init container may cause:
     - Marketing Automation plans may not work as Sitecore marketing definitions are not deployed.
     - Some Content Editor features and other admin pages relying on search indexes may not work.
     - The search page and search based components may not work on the CD.
5. Check the progress of the initialization by viewing the init container's logs:
   - `docker-compose logs -f init`
6. Wait about 30 minutes until the init container logs can read `No jobs are running. Monitoring stopped.`.
   - The init container has a known issue where it may stop itself before the jobs are all done. If you notice the init container has stopped before logging the `No jobs are running. Monitoring stopped.` message, restart the init container by running `docker-compose up -d` and continue monitoring its logs.

### Validating the Deployment

[top](#table-of-contents)

1. Browse to [https://cd.lighthouse.localhost](https://cd.lighthouse.localhost)
   1. You should see the Lighthouse landing page with a full-width carousel.
   2. If you do not see the full-width carousel and instead see the initial Sitecore default landing page, ensure that all the "init" container jobs are completed by checking its logs.
2. Browse to [https://cm.lighthouse.localhost/sitecore](https://cm.lighthouse.localhost/sitecore)
   1. You should be able to login with the "admin" user and the password provided while running the `init.ps1` script.
3. Browse to [http://127.0.0.1:44026/](http://127.0.0.1:44026/)
   1. You should see the SMTP container catch-all mailbox for all emails sent by EXM.

### Optional - Enable Coveo for Sitecore

[top](#table-of-contents)

There is an optional Coveo for Sitecore integration in the Lighthouse Demo.

Once Sitecore is up and running, you can [setup Coveo for Sitecore](/docs/Setup-coveo.md) on your instance.

## Stopping the demo

[top](#table-of-contents)

If you want to stop the demo without losing your changes:

1. Run `docker-compose stop`

At this point you can start the demo again with `docker-compose start` to continue your work where you left off.

## Starting Over

[top](#table-of-contents)

If you want to reset all of your changes and get a fresh instance:

1. Run `docker-compose down`
2. Run `.\CleanDockerData.ps1`
3. Start again with `docker-compose up -d` to have a fresh installation of Sitecore with no files/items deployed!

## Building the Demo

[top](#table-of-contents)

1. Open an elevated (as administrator) PowerShell session.
2. Navigate to your repository clone folder:
   - `cd C:\Projects\Sitecore.Demo.Platform`
3. Build your Docker images:
   - `.\build-images.ps1 -Memory 8G`
   - This command will:
     - Pull all the base images required by the dockerfiles.
       - You can use the `-SkipPull` switch to skip this step.
     - Build the demo images using the memory limit passed in the `-Memory` argument.
       - Adjust the number based on your available free memory.
       - The format is a number followed by the letter `G` for Gb. Same as the `--memory` argument of the `docker-compose build` command.
       - The `-Memory` argument is optional.

## Development Lifecycle

### Local Build Prerequisites

[top](#table-of-contents)

When building locally from Visual Studio or using the `.\build.ps1` script, you need some DLLs that are not available in NuGet packages. We created a script to pull them from Docker assets images.

1. Open an elevated (as administrator) PowerShell session.
2. Navigate to your repository clone folder:
   - `cd C:\Projects\Sitecore.Demo.Platform`
3. `.\pull-build-libraries.ps1`

### After changes to the code

[top](#table-of-contents)

1. Open an elevated (as administrator) PowerShell session.
2. Navigate to your repository clone folder:
   - `cd C:\Projects\Sitecore.Demo.Platform`
3. `.\build.ps1 -DeploymentTarget Docker`

This will build the solutions and deploy the build output to the `.\data\*\src` folders. The containers have volumes mounted to these `src` folders and are deploying any modified file directly to their `C:\inetpub\wwwroot` folders. Sitecore will shut down due to file changes. The container will still continue to run. The next HTTP request will restart Sitecore with the new DLLs and configuration files.

## Troubleshooting

[top](#table-of-contents)

### unauthorized: authentication required

**Problem:**

When running `docker-compose up -d`, you get the following error:

```text
ERROR: Get https://<registryname>/<someimage>/manifests/<someimage>: unauthorized: authentication required
```

**Cause:**

This indicates you are not logged in to your registry.

**Solution:**

Run `az acr login --name <registryname>` (or the equivalent `docker login`) and retry.

### manifest for scr.sitecore.com/build/lighthouse-solution:10.0.1-1001.0 not found

**Problem:**

When running `docker-compose build --pull`, you get the following error:

```text
ERROR: Service 'mssql' failed to build : manifest for scr.sitecore.com/build/lighthouse-solution:10.0.1-1001.0 not found: manifest unknown: manifest tagged by "10.0.1-1001.0" is not found
```

**Cause:**

The `--pull` switch tries to pull newer versions of all the base images used by the dockerfiles. The `lighthouse-solution` image is built by the `solution` service in the `docker-compose.override.yml` file. However, it is never pushed to the public Sitecore Docker registry.

**Solution:**

Do not use the `--pull` switch with the `docker-compose build` command. Instead, use the `.\build-images.ps1` script as instructed in this documentation. This script will pull the required base images first, then build the Docker images without the `--pull` switch.

### Issue downloading nodejs

**Problem:**

When running `.\build-images.ps1` or `docker-compose up -d`, you get an error about downloading nodejs.

**Cause:**

On some computers, containers are unable to resolve DNS entries. This issue is described in details in the following blog post: [https://development.robinwinslow.uk/2016/06/23/fix-docker-networking-dns/](https://development.robinwinslow.uk/2016/06/23/fix-docker-networking-dns/)

**Solution:**

Ensure the Windows Docker engine has DNS servers configured:

1. From the Docker Desktop taskbar icon contextual menu (right click), choose "Settings".
2. In the left tab group, navigate to the "Docker Engine" tab.
3. In the JSON block, locate the `"dns"` key.
   1. If you do not have a `"dns"` key, add it after the existing ones. Ensure you add a comma (`,`) after the previous key/value pair.
4. Ensure the value of the `"dns"` key is set to at least `["8.8.8.8"]`.
   - You can also add your ISP DNS server as instructed by the blog post.
5. At the end, the JSON block should have at least:

   ```json
   {
     "dns": ["8.8.8.8"]
   }
   ```

6. Click the "Apply & Restart" button to restart your Windows Docker engine.
7. Retry the command that resulted in the error.

### Windows Containers Unhealthy after compose

**Problem:**

Sitecore runs base images with IIS and ServiceMonitor in Hyper-V isolation.  

**Cause:**

The APPCMD process is what fails during compose.

In IISConfigUtil.cpp line 231 there is a 5-second timeout for APPCMD to complete. Containers that run only IIS may achieve this, but with Sitecore, this time limit is quite optimistic

[https://github.com/microsoft/IIS.ServiceMonitor](https://github.com/microsoft/IIS.ServiceMonitor)

**Solution:**

1. Find your Windows build number using Powershell `[Environment]::OSVersion.Version`
2. Use this [table](https://en.wikipedia.org/wiki/Windows_10_version_history) to get the version

   | Build | Version | Release Date |
   |-|-|-|
   | 17763 | 1809 | November 13, 2018 |
   | 18362 | 1903 | May 21, 2019 |
   | 18363 | 19H2 | November 12, 2019 |
   | 19041 | 20H1 | May 27, 2020 |
   | 19042 | 20H2 | October 20, 2020 |
   | 19043 | 21H1 | TBA |

3. `docker-compose down`  
4. Change the `WINDOWSSERVERCORE_VERSION` variable in `.env` to the version that matches your host system version
5. Change the `ISOLATION` variable to `process ` in the `.env` file
6. `docker-compose pull`
7. `docker-compose up -d`

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
| version | Version of Sitecore being targeted. Must match official 3-digit version. | `"10.0.1"` |
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
