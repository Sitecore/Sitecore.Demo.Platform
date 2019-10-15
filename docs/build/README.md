# Deployment Options

## Deploying locally with Docker

### Docker Deployment Prerequisites

- Sitecore base images built (either locally or in a registry)
  - see [Docker-Images Repo](https://github.com/sitecore/docker-images) for more details
- Windows version 1809 or later
- Docker (Engine) for Windows version 19.03 or later
- "az" PowerShell module
  - [https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows?view=azure-cli-latest](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows?view=azure-cli-latest)
- Requires **Team Development for Sitecore**

### Starting up your Sitecore instance

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

- Confirm that you can access the Sitecore instance deployed using docker-compose in the previous step by browsing to [http://127.0.0.1:44001/sitecore](http://127.0.0.1:44001/sitecore) which is the default endpoint for the CM role specified in the docker-compose.yml file. Ensure you replace it with your own value if you changed it!

- Review the `TDSGlobal.config` file if you've made any changes to the endpoints otherwise the defaults are fine.

**DEPLOY!**

`.\build.ps1 -Target Docker-TDS`

## Deploying to local IIS Site using TDS

### TDS Deployment Prerequisites

- Requires **Team Development for Sitecore**
- Requires a local working instance of Sitecore Experience Platform

### Deploying locally using TDS

- Confirm that you can access the Sitecore instance by browsing to [https://habitathome.dev.local/sitecore](https://habitathome.dev.local/sitecore) which is the default hostname when installing using the [Habitat Home Utilies](https://github.com/sitecore/sitecore.habitathome.utilities) repository. Ensure you replace it with your own value if you changed it!

**DEPLOY!**

`.\build.ps1 -Target Build-TDS`
