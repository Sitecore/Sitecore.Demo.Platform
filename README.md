# Introduction 
Habitat and the tools and processes in it is a Sitecore solution example built using Sitecore Experience Accelerator (SXA) following the Helix architecture principles.


# Getting Started
## Installation:

All installation instructions assume using PowerShell 5.1 in administrative mode.

### 1 Clone this repository
Clone the Sitecore.Habitat repository locally - defaults are configured for **C:\Projects\Sitecore.Habitat**. 

> #### Required if cloning to a folder **different than c:\projects**:
> 
> Once you have cloned the Sitecore.Habitat repository to the desired folder...
> 
> **Change Habitat DevSettings:**
> Make a copy of `\src\Project\Habitat\code\App_Config\Include\Project\z.Habitat.Website.DevSettings.config` and call it z.Habitat.Website.DevSettings.**user**.config
>  
> **DO NOT modify the existing configuration file**


- **https**:	`git clone https://sitecoredst.visualstudio.com/Demo/_git/Sitecore.Habitat` 
- **ssh**:		`git clone ssh://sitecoredst@vs-ssh.visualstudio.com:22/Demo/_ssh/Sitecore.Habitat`

### 2 Acquire packages
From the install directory

Execute **`.\get-latest-sitecore.ps1`**

### 3 Install Solr

From the install\solr directory, review and modify the install-solr.ps1 file as required
Execute **`install-solr.ps1`**

### 4 Set up Installation Configuration file
- Copy `set-installation-overrides.ps1.example` to `set-installation-overrides.ps1` and modify to match your environment 
- Ensure you **set the location of the license file** 
- Ensure the **solr details** in the overrides file **match your solr installation details**
- Execute **`.\set-installation-defaults.ps1`**
- Execute **`.\set-installation-overrides.ps1`** (modified from above)

At this point you should have a **`configuration-xp0.json`** file. Review the file for correctness.


### 5 Install Sitecore

- Run **`.\install-xp0.ps1`**
- This will install all required modules including SPE, SXA and Data Exchange Framework-related modules

### 7 Deploy Sitecore.Habitat
From the root directory of the solution
- Run **`npm install`**
- Run **`.\node_modules\.bin\gulp`** 

### 8 Deploy Sitecore.Habitat.Home
Clone the Sitecore.Habitat.Home repository locally - defaults are configured for **C:\Projects\Sitecore.Habitat.Home**

- **https**:	`git clone https://sitecoredst.visualstudio.com/Demo/_git/Sitecore.Habitat.Home` 
- **ssh**:		`git clone ssh://sitecoredst@vs-ssh.visualstudio.com:22/Demo/_ssh/Sitecore.Habitat.Home`


> #### Required if cloning to a folder **different than c:\projects**:
> 
> Once you have cloned the Sitecore.Habitat.Home repository to the desired folder...
> 
> **Change Habitat.Home DevSettings:**
>  Make a copy of `\src\Project\Habitat.Home\code\App_Config\Include\Project\z.Habitat.Home.Website.DevSettings.config` and call it z.Habitat.Home.Website.DevSettings.**user**.config
> 
> **DO NOT modify the existing configuration file**


From the root directory of Habitat Home solution:
- Run **`npm install`**
- Run **`.\node_modules\.bin\gulp`**

### 9 Modify hosts file and bindings
- Add habitathome.dev.local pointing to 127.0.0.1 to you hosts file
- Add habitathome.dev.local bindings (both HTTP and HTTPS) to IIS site bindings


# Contribute or Issues
Please post on Microsoft Teams:  **Teams - Sitecore Demo** if you would like contributor access to the repo or if you encounter any issues

