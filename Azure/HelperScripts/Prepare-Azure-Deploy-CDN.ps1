<#
.SYNOPSIS
This script prepares the environment for CDN Deployment

.PARAMETER ConfigurationFile
A cake-config.json file

#>

[CmdletBinding()]
Param(
	[parameter(Mandatory=$true, HelpMessage="Please Enter your cake-config.json")]
	[ValidateNotNullOrEmpty()]
    [string] $ConfigurationFile
)
Import-Module "$($PSScriptRoot)\ProcessConfigFile\ProcessConfigFile.psm1" -Force

$configarray     = ProcessConfigFile -Config $ConfigurationFile
$config          = $configarray[0]
$assetsfolder	 = $configarray[7]

#############################################
# Prepare the azuredeploy.json ARM Template
#############################################

$cdnJsonResource =  @"
{
    "apiVersion": "[variables('resourcesApiVersion')]",
    "name": "[concat(parameters('deploymentId'), '-infrastructure-cdn')]",
    "type": "Microsoft.Resources/deployments",
    "properties": {
      "mode": "Incremental",
      "templateLink": {
        "uri": "[concat(uri(parameters('templateLinkBase'), 'nested/infrastructure-cdn.json'), parameters('templateLinkAccessToken'))]"
      },
      "parameters": {
        "deploymentId": {
          "value": "[parameters('deploymentId')]"
        }
      }      
      
    }     
  }
"@

$azureDeployFile = $([io.path]::combine($assetsfolder, 'ArmTemplates', 'azuredeploy.json'))

if (!(Test-Path $azureDeployFile)) 
{
    Write-Host  "azuredeploy file '$($azureDeployFile)' not found." -ForegroundColor Red
    Write-Host  "Please ensure there is a azuredeploy.json file at '$($azureDeployFile)'" -ForegroundColor Red
    Exit 1
}   

$azureDeploy = Get-Content -Raw -Path $azureDeployFile | ConvertFrom-Json

[System.Collections.ArrayList] $resourcesArray = $azureDeploy.resources
$cdnJson = ConvertFrom-Json $cdnJsonResource 

$index = -1;

for ($i=0; $i -lt $resourcesArray.Count; $i++)
{    
    if ($resourcesArray[$i].name -match "cdn")
    {        
        $index = $i;
        break;
    }
   
}

if ($config.CDN -eq "true")
{ 
    if ($index -eq -1)
    {
        $resourcesArray.Insert(0, $cdnJson) 
    }    
}
else 
{
    $resourcesArray.RemoveAt($index)     
}

$azureDeploy.resources = $resourcesArray  

$azureDeploy | ConvertTo-Json -Depth 50 | ForEach-Object { [System.Text.RegularExpressions.Regex]::Unescape($_) } | set-content $azureDeployFile

#############################################
# Prepare the config files
#############################################