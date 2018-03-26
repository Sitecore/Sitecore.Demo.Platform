# Introduction 
Habitat and the tools and processes in it is a Sitecore solution example built using Sitecore Experience Accelerator (SXA) following the Helix architecture principles.

# Getting Started

## Prerequisites
The list of software / utilities is required as part of the install
 

### Windows Server 2016
*IF* you’re running a brand new instance of Windows Server 2016, you can download [features.xml](https://sitecore.box.com/s/365l8988xkr95i0i02funszvuf9him0y) to the machine and then run a script to install the features listed.

*The following will make the Windows Server 2016 Features “parity” to a sample generated the features.xml.*
**Using PowerShell as Administrator:**
`Import-Module ServerManager`
`Import-CliXml .\features.xml | Install-WindowsFeature`

### Software required

- [MS Build for Visual Studio 2017 with .NET core, .NET 4.6.2 and 4.7.1](https://www.visualstudio.com/thank-you-downloading-visual-studio/?sku=BuildTools&rel=15)
- [7Zip, Chrome and Notepad++](https://ninite.com/7zip-chrome-notepadplusplus/)
- [Git for Windows](https://github.com/git-for-windows/git/releases/download/v2.16.2.windows.1/Git-2.16.2-64-bit.exe)
- Java 8 U151 from [Oracle](http://www.oracle.com/technetwork/java/javase/downloads/java-archive-javase8-2177648.html "Oracle Download Archive")
	- Add Java folder to path
- [NodeJS](https://nodejs.org/dist/v8.10.0/node-v8.10.0-x64.msi) 


Restart-Computer once you have installed Java and NodeJS 

** 

## Custom install - before you start


The following is a list of default values / assumptions for install locations

**Habitat Project location**		`c:\projects\sitecore.habitat\`
**Habitat Home Project location**	`c:\projects\sitecore.habitat.home\`
**Habitat Site domain**				`habitat.dev.local`
**Web Root**						`c:\inetpub\wwwroot`
**Host Suffix**						`dev.local`

-- the wildcard host name *home.dev.local is used in the SXA Host Name (Site Grouping). 

If you do not use *home.dev.local you will need to modify the Host Name in 
`/sitecore/content/Habitat Sites/Habitat Home/Settings/Site Grouping/Habitat Home` after successfully deploying the site.
The Habitat Home site will not respond / render correctly until this value is modified. 

If you do **not want to use the default settings**, you need to adjust the appropriate values in the following files in **both** projects:

Create a user version of the following files

**Sitecore.Habitat**
`/gulp-config.user.js` 
`/publishsettings.user.targets` 
`/TDSGlobal.config.user` (only if using TDS)
`src\Project\Habitat\code\App_Config\Include\Project\z.Habitat.WebSite.DevSettings.user.config`

**Sitecore.Habitat.Home**
`/gulp-config.user.js` 
`/publishsettings.user.targets` 
`/TDSGlobal.config.user` (only if using TDS)
`\src\Project\Habitat.Home\code\App_Config\Include\Project\z.Habitat.Home.Website.DevSettings.user.config`

## Installation:

All installation instructions assume using PowerShell 5.1 in administrative mode.

### 1 Clone this repository
Clone the Sitecore.Habitat repository locally - defaults are configured for **C:\Projects\Sitecore.Habitat**. 

#### Setting Git for Long Paths

- Before cloning, you need to configure git to allow long paths, which is not the default.

`git config --system core.longpaths true`

- Clone 
-- **https**:	`git clone https://sitecoredst.visualstudio.com/Demo/_git/Sitecore.Habitat` 
-- **ssh**:		`git clone ssh://sitecoredst@vs-ssh.visualstudio.com:22/Demo/_ssh/Sitecore.Habitat`

> #### Required if cloning to a folder **different than c:\projects**:
> 
> Once you have cloned the Sitecore.Habitat repository to the desired folder...
> 
> **Change Habitat DevSettings:**
> Make a copy of `\src\Project\Habitat\code\App_Config\Include\Project\z.Habitat.Website.DevSettings.config` and call it z.Habitat.Website.DevSettings.**user**.config
>  
> **DO NOT modify the existing configuration file**


### 2 Acquire packages
From the install directory

Execute **`.\get-latest-sitecore.ps1`**

### 3 Install Java
Download and install Java SE Runtime Environment 8u151 from [Oracle](http://www.oracle.com/technetwork/java/javase/downloads/java-archive-javase8-2177648.html "Oracle Download Archive")
** After installation is complete, exit and re-open PowerShell.

### 4 Install Solr

From the install\solr directory, review and modify the install-solr.ps1 file as required
Execute **`install-solr.ps1`**


> Although it doesn't always happen, if you encounter an issue where **keytool cannot be found**, restart your computer.
 
### 5 Set up Installation Configuration file
- Copy `set-installation-overrides.ps1.example` to `set-installation-overrides.ps1` and modify to match your environment 
- Ensure you **set the location of the license file** 
- Ensure the **solr details** in the overrides file **match your solr installation details**
- Execute **`.\set-installation-defaults.ps1`**
- Execute **`.\set-installation-overrides.ps1`** (modified from above)

At this point you should have a **`configuration-xp0.json`** file. Review the file for correctness.

### 6 Install Sitecore

- Run **`.\install-xp0.ps1`**
- This will install all required modules including SPE, SXA and Data Exchange Framework-related modules

> The installation process takes a long time, please ensure you do not inadvertently click on the PowerShell window to make it go into **"Select" mode** (the word 'Select' appears in the PowerShell Window's title bar). If the window is in Select mode it **will not update** and will **prevent new steps from being executed**. 
> 
> To exit Select mode simply press Enter from inside the PowerShell window. **Clicking on the PowerShell window is what triggers the Select mode.**

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

> An error (maxBuffer) sometimes occurs the first time running gulp during Sync-Unicorn. 
> Running gulp a second time resolves the issue (and doesn't take as long)

### 9 Modify hosts file and bindings
- Add habitathome.dev.local pointing to 127.0.0.1 to you hosts file
- Add habitathome.dev.local bindings (both HTTP and HTTPS) to IIS site bindings

### 10 Rebuild indexes
Once you've confirmed the site has come up, please rebuild the master index.

### 11 Populate EXM Campaigns table
After deploying the site, navigate to `/sitecore/admin/MessageStatistics.aspx` and click **Upgrade**

# Failed installation

If you have a failed installation and need to clean up your deployment, you can execute the `uninstall-xp0.ps1` located in the install directory.

# Contribute or Issues
Please post on Microsoft Teams:  **Teams - Sitecore Demo** if you would like contributor access to the repo or if you encounter any issues

