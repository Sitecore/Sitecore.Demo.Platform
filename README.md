# Introduction 
Habitat and the tools and processes in it is a Sitecore solution example built using Sitecore Experience Accelerator (SXA) following the Helix architecture principles.

# Getting Started

## Prerequisites
The list of software / utilities is required as part of the install
 

### Windows Server 2016
*IF* you’re running a brand new instance of Windows Server 2016, you can download [features.xml](https://sitecore.box.com/s/365l8988xkr95i0i02funszvuf9him0y) to the machine and then run a script to install the features listed.

*The following will make the Windows Server 2016 Features “parity” to the generated features.xml.*

**Using PowerShell as Administrator:**
`Import-Module ServerManager`
`Import-CliXml <path-to-features.xml> | Install-WindowsFeature`

### Software required

- [MS Build for Visual Studio 2017 with .NET core, .NET 4.6.2 and 4.7.1](https://www.visualstudio.com/thank-you-downloading-visual-studio/?sku=BuildTools&rel=15)
- [7Zip, Chrome and Notepad++](https://ninite.com/7zip-chrome-notepadplusplus/) **
- [Git for Windows](https://github.com/git-for-windows/git/releases/download/v2.16.2.windows.1/Git-2.16.2-64-bit.exe)
- Java (JRE) 8 U151 from [Oracle](http://www.oracle.com/technetwork/java/javase/downloads/java-archive-javase8-2177648.html "Oracle Download Archive")
		- The file itself is called **jre-8u151-windows-x64.exe** (CTRL+F to find it quickly)
		- need a free Oracle account
	- Add Java bin folder to path
- [NodeJS](https://nodejs.org/dist/v8.10.0/node-v8.10.0-x64.msi) 
- VC++ 2015 Redistributable <a href="https://www.microsoft.com/en-us/download/details.aspx?id=53587&irgwc=1&OCID=AID681541_aff_7593_1211691&tduid=(ir_zTS3WeQr7Vf8xSI0HYRvRUkFUkj1QD24eQ9uTU0)(7593(1211691)(TnL5HPStwNw-plWV.mlI6FL23fFCV8WYiQ)(ir)&clickid=zTS3WeQr7Vf8xSI0HYRvRUkFUkj1QD24eQ9uTU0&ircid=7593">here</a>


Restart-Computer once you have installed Java and NodeJS 

** You can click [here: Ninite.com](https://ninite.com/7zip-chrome-notepadplusplus/) to install 7Zip, Chrome and Notepad all at once.

## Custom install - before you start


The following is a list of default values / assumptions for install locations

**Habitat Project location**		`c:\projects\sitecore.habitat\`
**Habitat Site domain**				`habitat.dev.local`
**Web Root**						`c:\inetpub\wwwroot`
**Host Suffix**						`dev.local`

-- the wildcard host name *home.dev.local is used in the SXA Host Name (Site Grouping). 

If you do not use *home.dev.local you will need to modify the Host Name in 
`/sitecore/content/Habitat Sites/Habitat Home/Settings/Site Grouping/Habitat Home` after successfully deploying the site.
The Habitat Home site will not respond / render correctly until this value is modified. 

If you do **not want to use the default settings**, you need to adjust the appropriate values in the following files in **both** projects:

Create a user version of the following files

**Sitecore.HabitatHome.Content**
`/gulp-config.user.js` 
`/publishsettings.targets.user` 
`/TDSGlobal.config.user` (only if using TDS)
`src\Project\Common\code\App_Config\Include\Project\z.Common.Website.DevSettings.user.config`


## Installation:

All installation instructions assume using PowerShell 5.1 in administrative mode.

### 1 Clone this repository
Clone the Sitecore.Habitat repository locally - defaults are configured for **C:\Projects\Sitecore.HabitatHome**. 

#### Setting Git for Long Paths

- Before cloning, you need to configure git to allow long paths, which is not the default.

`git config --system core.longpaths true`

- Clone 
-- **https**:	`git clone https://sitecoredst.visualstudio.com/Demo/_git/Sitecore.HabitatHome.Content` 
-- **ssh**:		`git clone ssh://sitecoredst@vs-ssh.visualstudio.com:22/Demo/_ssh/Sitecore.HabitatHome.Content`

> #### Required if cloning to a folder **different than c:\projects**:
> 
> Once you have cloned the Sitecore.HabitatHome.Content repository to the desired folder...
> 
> **Change DevSettings:**

> Make a copy of `\src\Project\Common\code\App_Config\Include\Project\z.Common.Website.DevSettings.config` and call it z.Common.Website.DevSettings.**user**.config
>  
> **DO NOT modify the existing configuration file**


### 2 Acquire packages
From the install directory

Execute **`.\get-latest-sitecore.ps1`**

### 3 Install Solr

From the install\solr directory, review and modify the install-solr.ps1 file as required
Execute **`install-solr.ps1`**


> Although it doesn't always happen, if you encounter an issue where **keytool cannot be found**, restart your computer.
 
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

> The installation process takes a long time, please ensure you do not inadvertently click on the PowerShell window to make it go into **"Select" mode** (the word 'Select' appears in the PowerShell Window's title bar). If the window is in Select mode it **will not update** and will **prevent new steps from being executed**. 
> 
> To exit Select mode simply press Enter from inside the PowerShell window. **Clicking on the PowerShell window is what triggers the Select mode.**

### 6 Give the application pool user permission to your webroot 
Browse to the root of the IIS (by default c:\inetpub\wwwroot) and give the Application Pool user read/write permissions.
Using Windows Explorer, navigate to c:\inetpub\wwwroot and in the Windows Security dialog, add IIS APPPOOL\habitat.dev.local (if that's the name of you application pool)

### 7 Deploy Sitecore.HabitatHome

From the root directory of the solution
- Run **`npm install`**
- Run **`.\node_modules\.bin\gulp`** 

> An error (maxBuffer) sometimes occurs the first time running gulp during Sync-Unicorn. 
> Running gulp a second time resolves the issue (and doesn't take as long)

### 9 Rebuild indexes
Once you've confirmed the site has come up, please rebuild the master index.

# Failed installation

If you have a failed installation and need to clean up your deployment, you can execute the `uninstall-xp0.ps1` located in the install directory.

# Contribute or Issues
Please post on Microsoft Teams:  **Teams - Sitecore Demo** if you would like contributor access to the repo or if you encounter any issues

