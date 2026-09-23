$ErrorActionPreference = 'Stop'
$source = Join-Path $PSScriptRoot 'AntigravityProxyLauncher.cs'
$output = Join-Path $PSScriptRoot 'AntigravityProxyLauncher.exe'
Add-Type -Path $source -OutputAssembly $output -OutputType WindowsApplication -ReferencedAssemblies @('System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll')
Write-Host "Built $output"
