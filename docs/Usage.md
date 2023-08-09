# Using Lighthouse Demo

## Table of Contents

- **[Clone this Repository](#clone-this-repository)**
- **[Prerequisites](#prerequisites)**
- **[Preparing Docker](#preparing-docker)**
- **[Preparing Your Environment](#preparing-your-environment)**
- **[Running the Demo](#running-the-demo)**
  - [Starting the Demo Containers](#starting-the-demo-containers)
  - [Validating the Deployment](#validating-the-deployment)
- **[Stopping the Demo](#stopping-the-demo)**
- **[Starting Over](#starting-over)**
- **[Building the Demo](#building-the-demo)**
- **[Development Lifecycle](#development-lifecycle)**
  - [After changes to the code](#after-changes-to-the-code)
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
- The [Docker Desktop](https://docs.docker.com/desktop/release-notes/#4180) v4.18.0 or v4.17.* - because of using LCOW Docker engine feature to run Windows and Linux containers simultaneously.

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

   6. Optionally, you may want to also set DNS servers in the Docker engine configuration. See the [Issue downloading nodejs](#issue-downloading-nodejs) known issue for details and instructions.
   7. Click the "Apply & Restart" button to restart your Windows Docker engine.

## Preparing Your Environment

[top](#table-of-contents)

1. Open an elevated (as administrator) PowerShell session.
2. Navigate to your repository clone folder:
   - `cd C:\Projects\Sitecore.Demo.Platform`
3. Create certificates and initialize the environment file:
   - `.\init.ps1 -InitEnv -LicenseXmlPath C:\license\license.xml -AdminPassword b`
   - You can change the admin password and the license.xml file path to match your needs.
   - **Note:** The admin username is set to "superuser" in this demo instead of the default "admin".

## Running the Demo

### Starting the Demo Containers

[top](#table-of-contents)

1. Open an elevated (as administrator) PowerShell session.
2. Navigate to your repository clone folder:
   - `cd C:\Projects\Sitecore.Demo.Platform`
3. Stop the IIS service: (_if applicable_)
   - `iisreset /stop`
   - This is required each time you want to use the demo as the Traefik container uses port (443), which is used by IIS.
4. Start the demo containers:
   - `.\up.ps1`
   - This will build all of the necessary images and start your Sitecore environment. It will take quite some time if this is the first time you execute it.
   - After starting the containers, the Sitecore instance will be up and available within minutes, but not fully working until the init container jobs are completed. The init container runs scripts that:
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
   - The init container has a known issue where it may stop itself before the jobs are all done. If you notice the init container has stopped before logging the `No jobs are running. Monitoring stopped.` message, restart the init container by running `.\up.ps1 -SkipBuild` and continue monitoring its logs.

### Validating the Deployment

[top](#table-of-contents)

1. Browse to [https://cd.lighthouse.localhost](https://cd.lighthouse.localhost)
   1. You should see the Lighthouse landing page with a full-width carousel.
   2. If you do not see the full-width carousel and instead see the initial Sitecore default landing page, ensure that all the "init" container jobs are completed by checking its logs.
2. Browse to [https://cm.lighthouse.localhost/sitecore](https://cm.lighthouse.localhost/sitecore)
   1. You should be able to login with the "**superuser**" user and the password "**b**" (or the one provided while running the `init.ps1` script).
3. Browse to [http://127.0.0.1:44026/](http://127.0.0.1:44026/)
   1. You should see the SMTP container catch-all mailbox for all emails sent by EXM.

## Stopping the demo

[top](#table-of-contents)

If you want to stop the demo without losing your changes:

1. Run `.\down.ps1`

At this point you can start the demo again with `.\up.ps1 -SkipBuild` to continue your work where you left off.

## Starting Over

[top](#table-of-contents)

If you want to reset all of your changes and get a fresh instance:

1. Run `.\down.ps1`
2. Run `.\CleanDockerData.ps1`
3. Start again with `.\up.ps1` to have a fresh installation of Sitecore with no files/items deployed!

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

### After changes to the code

[top](#table-of-contents)

1. Open an elevated (as administrator) "Developer PowerShell for VS 2019/2022" session.
   1. Regular PowerShell cannot run the msbuild command.
2. Navigate to your repository clone folder:
   - `cd C:\Projects\Sitecore.Demo.Platform`
3. `msbuild Sitecore.Demo.Platform.sln /t:restore,build /p:DeployOnBuild=true /p:PublishProfile=Local`

This will restore the NuGet packages, build the solutions, and deploy the build output to the `.\data\cm\src` and `.\data\xconnect\src` folders. The containers have volumes mounted to these `src` folders and are deploying any modified file directly to their `C:\inetpub\wwwroot` folders. Sitecore will shut down due to file changes. The container will still continue to run. The next HTTP request will restart Sitecore with the new DLLs and configuration files.

### Deploying to the CD container

The `Local` msbuild publish profile only deploys to the CM and xConnect containers. To deploy to the CD container:

1. Deploy to the CM container by executing the steps above in [After changes to the code](#after-changes-to-the-code).
2. Delete the content of the `.\data\cd\src` folder.
3. Copy the content of the `.\data\cm\src` folder into the `.\data\cd\src` folder.

The CD container have a volume mounted to this `.\data\cd\src` folder and is deploying any modified file directly to its `C:\inetpub\wwwroot` folder. Sitecore will shut down due to file changes. The container will still continue to run. The next HTTP request will restart Sitecore with the new DLLs and configuration files.

### After changes to the xConnect files

While the xConnect container gets published files, the xdbsearchworker container does not get them. When you do changes to any xConnect model or settings, do the following:

1. Open an elevated (as administrator) PowerShell session.
2. Navigate to your repository clone folder:
   - `cd C:\Projects\Sitecore.Demo.Platform`
3. Rebuild the solution Docker image:
   - `.\build-images.ps1 -Services solution -Memory 8G`
4. Rebuild xconnect and xdb related Docker images:
   - `.\build-images.ps1 -Services xconnect,xdbsearchworker,xdbautomationworker -Memory 8G`
5. Restart the containers using the newly built Docker images:
   - `docker-compose up -d`

### After change to the XDT files

XML files are not transformed inside the containers after new XDT files are published to them. You must rebuild the CM and CD Docker images when modifying XDT files:

1. Open an elevated (as administrator) PowerShell session.
2. Navigate to your repository clone folder:
   - `cd C:\Projects\Sitecore.Demo.Platform`
3. Rebuild the solution Docker image:
   - `.\build-images.ps1 -Services solution -Memory 8G`
4. Rebuild the CM and CD Docker images:
   - `.\build-images.ps1 -Services cm,cd -Memory 8G`
5. Restart the containers using the newly built Docker images:
   - `docker-compose up -d`

## Troubleshooting

[top](#table-of-contents)

### unauthorized: authentication required

**Problem:**

When running `.\up.ps1`, you get the following error:

```text
ERROR: Get https://<registryname>/<someimage>/manifests/<someimage>: unauthorized: authentication required
```

**Cause:**

This indicates you are not logged in to your Docker registry.

**Solution:**

Run `az acr login --name <registryname>` (or the equivalent `docker login`) and retry.

### manifest for scr.sitecore.com/build/lighthouse-solution:10.0.1-1001.0 not found

**Problem:**

When running `docker-compose build --pull`, you get the following error:

```text
ERROR: Service 'mssql' failed to build : manifest for scr.sitecore.com/build/lighthouse-solution:10.0.1-1001.0 not found: manifest unknown: manifest tagged by "10.0.1-1001.0" is not found
```

**Cause:**

The script tries to pull newer versions of all the base images used by the dockerfiles. The `lighthouse-solution` image is built by the `solution` service in the `docker-compose.override.yml` file. However, it is never pushed to the public Sitecore Docker registry.

**Solution:**

Do not use the `--pull` switch with the `docker-compose build` command. Instead, use the `.\build-images.ps1` script as instructed in this documentation. This script will pull the required base images first, then build the Docker images without the `--pull` switch.

### Issue downloading nodejs

**Problem:**

When running `.\build-images.ps1` or `.\up.ps1`, you get an error about downloading nodejs.

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

### Windows Containers Unhealthy After Compose

**Problem:**

Sitecore runs base images with IIS and ServiceMonitor in Hyper-V isolation.

**Cause:**

The APPCMD process is what fails during compose.

In IISConfigUtil.cpp line 231 there is a 5-second timeout for APPCMD to complete. Containers that run only IIS may achieve this, but with Sitecore, this time limit is quite optimistic.

[https://github.com/microsoft/IIS.ServiceMonitor](https://github.com/microsoft/IIS.ServiceMonitor)

**Solution:**

1. Find your Windows build number using Powershell:

   ```powershell
   [Environment]::OSVersion.Version
   ```

2. Use this [table](https://en.wikipedia.org/wiki/Windows_10_version_history) to get the version

   | Build | Version | Release Date |
   |-|-|-|
   | 17763 | 1809 | November 13, 2018 |
   | 18362 | 1903 | May 21, 2019 |
   | 18363 | 19H2 | November 12, 2019 |
   | 19041 | 20H1 | May 27, 2020 |
   | 19042 | 20H2 | October 20, 2020 |
   | 19043 | 21H1 | TBA |

3. `.\down.ps1`
4. In `.env` file:
   1. Change the `WINDOWSSERVERCORE_VERSION` variable value to the version that matches your host system version.
   2. Change the `ISOLATION` variable value to `process`
5. `.\up.ps1`

### Disable Unicorn Serialization

When Unicorn is active, the Content Editor will display warnings that certain items are controlled by Unicorn. If you wish to disable Unicorn serialization:

1. Open the `docker-compose.override.yml` file in your repository clone.
2. Find all occurrences of the `SITECORE_APPSETTINGS_UNICORN:DEFINE` environment variable.
3. Update all occurences to `SITECORE_APPSETTINGS_UNICORN:DEFINE: Disabled`
4. Save the file.
5. Run `.\up.ps1 -SkipBuild` again to update the containers.

This setting is set to `Enabled` by default. Setting it to `Disabled` ensures that none of the Unicorn serialization configuration files are loaded.
