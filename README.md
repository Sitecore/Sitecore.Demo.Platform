# Introduction 
Habitat and the tools and processes in it is a Sitecore solution example built using Sitecore Experience Accelerator (SXA) following the Helix architecture principles.
The architecture and methodology focuses on:

Simplicity - A consistent and discoverable architecture
Flexibility - Change and add quickly and without worry
Extensibility - Simply add new features without steep learning curve

# Getting Started
## Installation:

### 1 Clone this repository
Clone the Sitecore.Habitat repository locally - scripts assume c:\Projects\Sitecore.Habitat
- **https**:	`git clone https://sitecoredst.visualstudio.com/Demo/_git/Sitecore.Habitat` 
- **ssh**:		`git clone ssh://sitecoredst@vs-ssh.visualstudio.com:22/Demo/_ssh/Sitecore.Habitat`

### 2 Acquire packages

Download the following packages

- [Sitecore 9.0.0 rev. 171002](https://dev.sitecore.net/Downloads/Sitecore_Experience_Platform/90/Sitecore_Experience_Platform_90_Initial_Release.aspx) 
   - Select Packages for XP Single under "Deployment options for On Premises deployment")
* [Sitecore PowerShell Extensions-4.7 for Sitecore 8.zip](https://marketplace.sitecore.net/Modules/S/Sitecore_PowerShell_console.aspx)
* [Sitecore Experience Accelerator 1.5 rev. 171010 for 9.0.zip](https://dev.sitecore.net/Downloads/Sitecore_Experience_Accelerator/15/Sitecore_Experience_Accelerator_15_Initial_Release.aspx)

### 3 Extract Packages
Extract Contents of Sitecore 9.0.0 rev. 171002 (WDP XP0 packages).zip to ./build/assets

### 4 Copy License
Copy license file to `./build/assets`

### 5 Install Solr

### 6 Install Sitecore
- Review settings.ps1 file. If any modifications need to be made, create a new settings-user.ps1 and override the settings.
- Open PowerShell window with Administrative Privileges
- Run **`.\install-xp0.ps1`**
- Install package **Sitecore PowerShell Extensions 4.7**
- Install package **Sitecore Experience Accelerator 1.5**

### 7 Deploy Solution
- Run `npm install`
- Run `gulp` 

# Contribute
Please contact jfl on Microsoft Teams:  **Teams - Sitecore Demo** if you would like contributor access to the repo

