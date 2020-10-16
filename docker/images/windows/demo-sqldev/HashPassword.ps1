Param (
  [object]$Password,
  [byte[]]$byteSalt #Optional
)
$algorithm = 'SHA512'
#Generate salt
if (!$byteSalt) {
  0 .. 15 | ForEach-Object { $byteSalt += ([Math]::Floor($(Get-Random -Minimum 0 -Maximum 255))) }
}
#Extract password
if ($Password.GetType().Name -eq 'SecureString') {
  $cred = New-Object System.Management.Automation.PSCredential -ArgumentList 'foo', $Password
  $plainPassword = $cred.GetNetworkCredential().Password
}
else {
  $plainPassword = $Password
}
#Get byte representation of the password string
$enc = [system.Text.Encoding]::Unicode
$data = $enc.GetBytes($plainPassword)
#Run hash algorithm
$hash = [Security.Cryptography.HashAlgorithm]::Create($algorithm)
$hashedPassword = $hash.ComputeHash($byteSalt + $data)
$encodedPassword = [Convert]::ToBase64String($hashedPassword)

$encodedSalt = [Convert]::ToBase64String($byteSalt)
$result = @{
  Password = $encodedPassword.replace("=", "'+CHAR(61)+'")
  Salt     = $encodedSalt.replace("=", "'+CHAR(61)+'")
}
return $result