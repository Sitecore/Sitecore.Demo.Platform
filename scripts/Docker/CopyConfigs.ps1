Copy-Item C:\inetpub\wwwroot\Web.config -Destination C:\src 


$destinationFolder = "C:\src\App_Config"

if (!(Test-Path -path $destinationFolder)) {New-Item $destinationFolder -Type Directory}

Copy-Item C:\inetpub\wwwroot\App_Config\Layers.config $destinationFolder