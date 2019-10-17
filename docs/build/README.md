# Deployment Options

## Deploying Locally With Docker

### Docker Deployment Known Issues

- xConnect is currently not working (throwing xConnect model errors)
- Need to manually run "Populate Solr Managed Schemas" in the Control Panel before Solr indexing works

### Docker Deployment Prerequisites

- Sitecore base images built (either locally or in a registry)
  - see [Docker-Images Repo](https://github.com/sitecore/docker-images) for more details
- Windows version 1809 or later
- Docker (Engine) for Windows version 19.03 or later
- "az" PowerShell module
  - [https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows?view=azure-cli-latest](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows?view=azure-cli-latest)

### Starting Up Your Sitecore Instance

- Ensure your Sitecore license is in `c:\license`
- Modify .env file:
  - `REGISTRY`: set your registry url (trailing slash (/) is important).
  - `REMOTEDEBUGGER_PATH`: Ensure the path is valid. You might have to change `Enterprise` to `Professional` or `Community`.
- Set the SA password to the one configured in your base image
- login to your docker registry
  - for Azure ACR:
    - `az login`
    - `az acr login --name <registryname>`
- run `docker-compose up -d`
  - this will pull all of the necessary base images and spin up your Sitecore environment. It will take **quite some time** if this is the first time you execute it.

### Deploying to Docker

Confirm that you can access the Sitecore instance deployed using docker-compose in the previous step by browsing to [http://127.0.0.1:44001/sitecore](http://127.0.0.1:44001/sitecore) which is the default endpoint for the CM role specified in the docker-compose.yml file. Ensure you replace it with your own value if you changed it!

#### Deploying to Docker With Unicorn

1. Review the `cake-config.json` file if you've made any changes to the endpoints or if you need to change the default settings.
1. Smart publish the site.
1. Run `.\build.ps1 -Target Docker-Unicorn`
1. Run Docker post-deployment steps below.

#### Deploying to Docker With TDS

**Note:** Requires **Team Development for Sitecore**

1. Review the `TDSGlobal.config` file if you've made any changes to the endpoints otherwise the defaults are fine.
1. Run `.\build.ps1 -Target Docker-TDS`
1. Run Docker post-deployment steps below.

### Docker Post-Deployment Steps

1. Open the Content Editor
1. Navigate to the `/sitecore/content/Habitat SXA Sites/Habitat Home/Settings/Site Grouping/Habitat Home` item
1. Change the value of the `Host Name` field to `*`
1. Save the item
1. Smart publish the site in all languages

### Cleaning and Re-deploying With Docker

In case you want to start over.

1. Run `docker-compose down`
2. Run `.\CleanDockerData.ps1`
3. At this point you can start again with `docker-compose up -d` to have a fresh installation of Sitecore with no files/items deployed!

## Deploying to Local IIS Site

### IIS Site Local Deployment Prerequisites

1. Requires a local working instance of Sitecore Experience Platform
1. Confirm that you can access the Sitecore instance by browsing to [https://habitathome.dev.local/sitecore](https://habitathome.dev.local/sitecore) which is the default hostname when installing using the [Habitat Home Utilies](https://github.com/sitecore/sitecore.habitathome.utilities) repository. Ensure you replace it with your own value if you changed it!

### Deploying to Local IIS Site Using TDS

**Note:** Requires **Team Development for Sitecore**

1. Run `.\build.ps1 -Target Build-TDS`

### Deploying to Local IIS Site Using Unicorn

1. Run `.\build.ps1`
