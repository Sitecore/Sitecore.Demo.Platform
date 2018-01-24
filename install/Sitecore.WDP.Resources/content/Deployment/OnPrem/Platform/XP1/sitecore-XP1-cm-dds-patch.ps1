function Invoke-TransformXmlDoc (
	[Parameter(Mandatory=$true,helpmessage="Root Directory Path")]
	[string]$path,
	[Parameter(Mandatory=$true,helpmessage="XML Transformation Documents Directory")]
	[alias("XmlDelta", "XmlTransfrom")]
	[string]$xdtDirectory
)
{
	nuget install Microsoft.Web.Xdt -x -outputdirectory $PSScriptRoot
    Add-Type -Path (Get-ChildItem -Filter "*.dll" -Recurse | ?{$_.Name -eq "Microsoft.Web.XmlTransform.dll"} | Select-Object -First 1).FullName

    Get-ChildItem $xdtDirectory -Recurse -Filter "*.xdt" | %{
        
        $xml = Join-Path $Path (($_.FullName -replace [regex]::Escape($xdtDirectory),'') -replace ".xdt",'')

        if (!(Test-Path -path $xml -PathType Leaf)) {
            throw "File not found. $xml";
        }

        $xmldoc = New-Object Microsoft.Web.XmlTransform.XmlTransformableDocument;
        $xmldoc.PreserveWhitespace = $true
        $xmldoc.Load($xml);

        Copy-Item $xml "$Xml.bak"

        $transf = New-Object Microsoft.Web.XmlTransform.XmlTransformation($_.FullName);
        if ($transf.Apply($xmldoc) -eq $false)
        {
            throw "Transformation failed."
        }
        $xmldoc.Save($xml);
    }
}

function Invoke-IoXml (
	[Parameter(Mandatory=$true,helpmessage="Root Directory Path")]
	[string]$path,
	[Parameter(Mandatory=$true,helpmessage="IO XML Document")]
	[string]$ioXmlPath
)
{
    [xml]$ioXml = Get-Content $ioXmlPath
    
    $ioXml.IOActions | %{
        switch ($_.IOAction.action) {
            "enable" { Rename-Item (Join-Path $path $_.IOAction.path) ((Join-Path $path $_.IOAction.path) -replace ".disabled", "") }
            "disable" { Rename-Item (Join-Path $path $_.IOAction.path) "$(Join-Path $path $_.IOAction.path).disabled" }
            "delete" { Remove-Item (Join-Path $path $_.IOAction.path) -Force}
        }
    }
}

Register-SitecoreInstallExtension -Command Invoke-TransformXmlDoc -As TransformXmlDoc -Type Task -Force
Register-SitecoreInstallExtension -Command Invoke-IoXml -As IoXml -Type Task -Force