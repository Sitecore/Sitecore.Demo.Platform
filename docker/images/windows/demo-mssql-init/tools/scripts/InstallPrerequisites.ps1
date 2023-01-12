#14.0
Invoke-WebRequest -Uri "https://download.microsoft.com/download/f/1/9/f19eaee6-0728-4a0b-9755-9808acc8af0b/EN/x64/DacFramework.msi" -OutFile DacFramework.msi
Invoke-WebRequest -Uri "https://aka.ms/vs/15/release/vc_redist.x64.exe" -OutFile vc_redist.x64.exe
Invoke-WebRequest -Uri "https://go.microsoft.com/fwlink/?linkid=2120137" -OutFile msodbcsql.msi
#14.0
Invoke-WebRequest -Uri "https://download.microsoft.com/download/4/A/3/4A323490-8EC0-48AE-9F22-638AA6C508C6/EN/x64/MsSqlCmdLnUtils.msi" -OutFile MsSqlCmdLnUtils.msi
Start-Process -NoNewWindow -Wait msiexec.exe -ArgumentList /i, DacFramework.msi, /quiet, /norestart
Start-Process -NoNewWindow -Wait vc_redist.x64.exe -ArgumentList /install, /quiet, /norestart
Start-Process -NoNewWindow -Wait msiexec.exe -ArgumentList /i, msodbcsql.msi, /quiet, /norestart, "IACCEPTMSODBCSQLLICENSETERMS=YES"
Start-Process -NoNewWindow -Wait msiexec.exe -ArgumentList /I, "MsSqlCmdLnUtils.msi", /QN, "IACCEPTMSSQLCMDLNUTILSLICENSETERMS=YES"
Remove-Item -Force DacFramework.msi, vc_redist.x64.exe, msodbcsql.msi, MsSqlCmdLnUtils.msi

$env:Path += ";C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn"
$env:Path += ";C:\Program Files\Microsoft SQL Server\150\DAC\bin"
[Environment]::SetEnvironmentVariable("Path", $env:Path, [System.EnvironmentVariableTarget]::Machine)