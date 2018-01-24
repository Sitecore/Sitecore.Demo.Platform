# Installation Configuration

### Overview

The installation process leverages Sitecore Installation Framework (SIF) to deploy Sitecore XP. A script has been provided to _bootstrap_ the SIF installation steps and configuration.



- `install-xp0.ps1` - script used to load a configuration file and call SIF. The main purpose of this script is to validate the prerequisites and organize the various calls to SIF.

- `install-settings.json` - this is the "template" file which holds placeholders for all configurable values. This file is used by the set-installation*.ps1 to generate a properly filled out configuration file. The @@value@@ is designed to be used with token replacement tools which are normally used in build systems.

- `set-installation-defaults.ps1` - execute this file to generate a new configuration json file - completed with defaults - that can be used with `install-xp0.ps1`. This script will use `install-settings.json` file as a template.

- `set-installation-overrides.ps1.example` - Modify this file to include the values you want to override. Specify a -ConfigurationFile generated in after calling set-installation-defaults.ps1 (install-xp0.json is the default value)



### Setting up the parameter values



1. Execute `.\set-installation-defaults.ps1`
 
	This will generate a file named install-xp0.json (by default - you can override the file name by executing `.\set-installation-defaults.ps1 -ConfigurationFile <config-file-name.json>` and specifying the ConfigurationFile parameter when calling the script).

2. To avoid modifying the base `set-installation-defaults.ps1` file, you can remove the .example extension on the `set-installation-overrides.ps1.example` file. Make any modifications to your settings here and execute this file after first executing the defaults file. Only values you wish to modify / override need to be specified in this file.

3. Execute `install-xp0.ps1 [-ConfigurationFile <your-generate-config-file.json>]` 
