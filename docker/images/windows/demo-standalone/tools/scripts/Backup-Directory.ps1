[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory = $true)]
    [ValidateScript( { Test-Path $_ -PathType 'Container' })]
    [string]$Path,

    [Parameter(Mandatory = $true)]
    [ValidateScript( { Test-Path $_ -PathType 'Container' })]
    [string]$Destination,

    [Parameter(Mandatory = $false)]
    [int]$Sleep = 300000,

    [Parameter(Mandatory = $false)]
    [int]$Timeout = 0,

    [Parameter(Mandatory = $false)]
    [array]$DefaultExcludedFiles = @("*.user", "*.cs", "*.csproj", "packages.config", "*ncrunch*", ".gitignore", ".gitkeep", ".dockerignore", "*.example", "*.disabled"),

    [Parameter(Mandatory = $false)]
    [array]$ExcludeFiles = @(),

    [Parameter(Mandatory = $false)]
    [array]$DefaultExcludedDirectories = @("obj", "Properties", "node_modules"),

    [Parameter(Mandatory = $false)]
    [array]$ExcludeDirectories = @()
)

# Setup
$ErrorActionPreference = "Stop"
$timeFormat = "HH:mm:ss:fff"

# Setup exclude rules
$fileRules = ($DefaultExcludedFiles + $ExcludeFiles) | Select-Object -Unique
$directoryRules = ($DefaultExcludedDirectories + $ExcludeDirectories) | Select-Object -Unique

Write-Information "$(Get-Date -Format $timeFormat): Excluding files: $($fileRules -join ", ")"
Write-Information "$(Get-Date -Format $timeFormat): Excluding directories: $($directoryRules -join ", ")"

function Sync {
    param(
        [Parameter(Mandatory = $true)]
        $Path,
        [Parameter(Mandatory = $true)]
        $Destination,
        [Parameter(Mandatory = $false)]
        $ExcludeFiles,
        [Parameter(Mandatory = $false)]
        $ExcludeDirectories
    )

    $command = @("robocopy", "`"$Path`"", "`"$Destination`"", "/MIR", "/MT:4", "/NJH", "/NJS", "/FP", "/NDL", "/NFL", "/NP", "/NS", "/R:5", "/W:5")

    if ($ExcludeDirectories.Count -gt 0) {
        $command += "/XD "

        $ExcludeDirectories | ForEach-Object {
            $command += "`"$_`" "
        }

        $command = $command.TrimEnd()
    }

    if ($ExcludeFiles.Count -gt 0) {
        $command += "/XF "

        $ExcludeFiles | ForEach-Object {
            $command += "`"$_`" "
        }

        $command = $command.TrimEnd()
    }

    $commandString = $command -join " "

    $dirty = $false
    $raw = &([scriptblock]::create($commandString))
    $raw | ForEach-Object {
        $line = $_.Trim().Replace("`r`n", "").Replace("`t", " ")
        $dirty = ![string]::IsNullOrEmpty($line)

        if ($dirty) {
            Write-Information "$(Get-Date -Format $timeFormat): $line"
        }
    }

    if ($dirty) {
        Write-Information "$(Get-Date -Format $timeFormat): Done syncing..."
    }
}


Write-Information "$(Get-Date -Format $timeFormat): Watching '$Path' for changes, will copy to '$Destination'..."

# Main loop
$timer = [System.Diagnostics.Stopwatch]::StartNew()
while ($Timeout -eq 0 -or $timer.ElapsedMilliseconds -lt $Timeout) {
    Sync -Path $Path -Destination $Destination -ExcludeFiles $fileRules -ExcludeDirectories $directoryRules

    Start-Sleep -Milliseconds $Sleep
}

