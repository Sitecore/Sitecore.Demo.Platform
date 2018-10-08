$thumbprint = (New-SelfSignedCertificate `
    -Subject "CN=<FULLY QUALIFIED DOMAIN NAME>" `
    -Type SSLServerAuthentication `
    -FriendlyName "<NAME>").Thumbprint

$certificateFilePath = "<FULL PATH TO PFX FOLDER>\$thumbprint.pfx"
Export-PfxCertificate `
    -cert cert:\LocalMachine\MY\$thumbprint `
    -FilePath "$certificateFilePath" `
    -Password (Read-Host -Prompt "Enter password that would protect the certificate" -AsSecureString)