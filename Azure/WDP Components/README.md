--- Module Web Deploy Package (WDP) creation How-To ---

PowerShell command to create WDP out of a Sitecore package .zip file:

Start-SitecoreAzureModulePackaging

It takes the following parameters (and will fail if any of them is empty string):

	-SourceFolderPath - Path to the folder where the Sitecore module package file is located
	-DestinationFolderPath - Path to the folder where the output WDP will go
	-CargoPayloadFolderPath - Path to the folder, containing the module's Cargo Payload package (.sccpl) file
	-AdditionalWdpContentsFolderPath - (Not used) Path to additional files that we want to include to the WDP
	-ParameterXmlFolderPath - Path to a parameters xml file for the module that describes IIS website and/or database strings
	-ConfigFilePath - Path to the JSON file which lists information about the source Sitecore module file, the Cargo Payload package, 
						the parameters.xml file and any additional files that need to be described for the WDP.
						It can also include more than one Sitecore module files - the result would be more than one WDP files created by the Azure Toolkit

EXAMPLE:

Import-Module "C:\Users\auzunov\Downloads\ARM_deploy\1_Sitecore Azure Toolkit\tools\Sitecore.Cloud.Cmdlets.psm1" -Verbose
Start-SitecoreAzureModulePackaging -SourceFolderPath "C:\Users\auzunov\Downloads\ARM_deploy\Modules\DAFx\DAFx_zip" `
                                   -DestinationFolderPath "C:\Users\auzunov\Downloads\ARM_deploy\Modules\DAFx\WPD" `
                                   -CargoPayloadFolderPath "C:\Users\auzunov\Downloads\ARM_deploy\Modules\DAFx\Components\CargoPayloads" `
                                   -AdditionalWdpContentsFolderPath "C:\Users\auzunov\Downloads\ARM_deploy\Modules\DAFx\Components\AdditionalFiles" `
                                   -ParameterXmlFolderPath "C:\Users\auzunov\Downloads\ARM_deploy\Modules\DAFx\Components\MsDeployXmls" `
                                   -ConfigFilePath "C:\Users\auzunov\Downloads\ARM_deploy\Modules\DAFx\Components\Configs\TestDAFx.config.json" `
                                   -Verbose

Related links:

Azure Deployment Toolkit overview
https://doc.sitecore.net/cloud/working_with_sitecore_azure_toolkit/overview/getting_started_with_sitecore_azure_toolkit

WDP creation overview
https://doc.sitecore.net/cloud/working_with_sitecore_azure_toolkit/packaging/packaging_a_sitecore_solution_for_the_microsoft_azure_app_service

Sitecore Cargo Payload File
https://doc.sitecore.net/cloud/working_with_sitecore_azure_toolkit/packaging/the_structure_of_an_sccpl_transformation

Example of SXA module preparation and deployment
https://doc.sitecore.net/sitecore_experience_accelerator/13/setting_up_and_configuring/setting_up/configure_sxa_for_deployment_on_the_azure_app_service

How to create custom Sitecore Cargo Payload packages (SCCPLs):

	1. Create a CargoPayloads folder
	2. Underneath, there can be four sub-folders:
		a. CopyToWebsite - serves as folder container for files that need to be copied out to the webroot on deployment
		b. CopyToRoot - serves as folder container for files that need to reside in the root of the WebDeploy package folder
		c. Xdts - serves as folder container for XDT transform files that will be used during the deployment of the package
		d. IOActions - contains special xml files with extension .ioxml that serve to describe what happens with files and folders during the deployment (if they are deleted etc)
	3. Create each of these folders, depending on the usage you want (I went with empty folders first)
	4. Zip up the CargoPayloads folder
	5. Rename the resulting .zip file extension to .sccpl