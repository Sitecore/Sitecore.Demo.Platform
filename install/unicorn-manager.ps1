Param(
[string]$command
)
$config = Get-Content -Raw -Path "$PSSCriptRoot\unicorn.json" | ConvertFrom-Json
$root = "C:\websites\habitat.dev.local\Website"

if ($command -eq "disable") {
	foreach ($action in $config.roles.disable.IOActions)
		{
			$path = $root + "\" + $action.path
			if ($action.path -notmatch ".demo") {        
				Rename-Item $path "$path.demo" -Verbose -ErrorAction Continue
			}
		}
}

if ($command -eq "enable") {
	foreach ($action in $config.roles.enable.IOActions)
		{
			$path = $root + "\" + $action.path
			if ($command -eq "enable") {    
				$enabledPath = $path.Replace(".demo", "");
				Rename-Item $path "$enabledPath" -Verbose -ErrorAction Continue
			}
		}
}