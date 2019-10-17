Copy-Item C:\inetpub\sc\Web.config -Destination C:\src 


$destinationFolder = "C:\src\App_Config"

if (!(Test-Path -path $destinationFolder)) {New-Item $destinationFolder -Type Directory}

Copy-Item C:\inetpub\sc\App_Config\Layers.config $destinationFolder