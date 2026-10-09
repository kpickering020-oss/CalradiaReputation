
# PowerShell deployment script for local Bannerlord builds
param(
    [string]$GameRoot = "D:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord",
    [string]$ModuleName = "CalradiaReputation",
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $GameRoot)) {
    throw "Bannerlord install not found at $GameRoot"
}

$binPath = Join-Path $GameRoot "Modules\$ModuleName\bin\Win64_Shipping_Client"
$moduleRoot = Join-Path $GameRoot "Modules\$ModuleName"

if (-not (Test-Path $moduleRoot)) {
    New-Item -ItemType Directory -Path $moduleRoot -Force | Out-Null
}

if (-not (Test-Path $binPath)) {
    New-Item -ItemType Directory -Path $binPath -Force | Out-Null
}

$projectDll = Join-Path $PSScriptRoot "src\CalradiaReputation\bin\$Configuration\CalradiaReputation.dll"
if (-not (Test-Path $projectDll)) {
    throw "Build output missing at $projectDll. Build the project before deployment."
}

Copy-Item -Path $projectDll -Destination $binPath -Force

$moduleXml = Join-Path $PSScriptRoot "SubModule.xml"
Copy-Item -Path $moduleXml -Destination (Join-Path $moduleRoot "SubModule.xml") -Force

Write-Host "CalradiaReputation deployed to $moduleRoot"
