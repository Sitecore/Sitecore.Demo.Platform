Param(
[string]$command,
[string]$WebRoot = "C:\inetpub\wwwroot\habitat.dev.local"
)
$config = Get-Content -Raw -Path "$PSSCriptRoot\unicorn.json" | ConvertFrom-Json

if ($command -eq "disable") {
	foreach ($action in $config.roles.disable.IOActions)
		{
			$path = $WebRoot + "\" + $action.path
			if ($action.path -notmatch ".demo") {        
				Rename-Item $path "$path.demo" -Verbose -ErrorAction Continue
			}
		}
}

if ($command -eq "enable") {
	foreach ($action in $config.roles.enable.IOActions)
		{
			$path = $WebRoot + "\" + $action.path
			if ($command -eq "enable") {    
				$enabledPath = $path.Replace(".demo", "");
				Rename-Item $path "$enabledPath" -Verbose -ErrorAction Continue
			}
		}
}