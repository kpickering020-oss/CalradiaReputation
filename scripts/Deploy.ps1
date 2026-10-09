param(
    [string]$GameDir = $env:BANNERLORD_GAME_DIR,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

function Find-BannerlordDir {
    param([string]$Hint)
    if ($Hint -and (Test-Path -LiteralPath (Join-Path $Hint "Modules"))) {
        return $Hint
    }

    $overrideFile = Join-Path $repoRoot "BannerlordPath.txt"
    if (Test-Path $overrideFile) {
        $fromFile = (Get-Content $overrideFile -Raw).Trim()
        if ($fromFile -and (Test-Path -LiteralPath (Join-Path $fromFile "Modules"))) {
            return $fromFile
        }
    }

    $libraryFiles = @(
        "${env:ProgramFiles(x86)}\Steam\steamapps\libraryfolders.vdf",
        "${env:ProgramFiles}\Steam\steamapps\libraryfolders.vdf"
    )
    foreach ($libraryFile in $libraryFiles) {
        if (-not (Test-Path -LiteralPath $libraryFile)) {
            continue
        }
        $text = Get-Content -LiteralPath $libraryFile -Raw
        $paths = [regex]::Matches($text, '"path"\s+"([^"]+)"') | ForEach-Object { $_.Groups[1].Value -replace '\\\\', '\' }
        foreach ($library in $paths) {
            $candidate = Join-Path $library "steamapps\common\Mount & Blade II Bannerlord"
            if (Test-Path -LiteralPath (Join-Path $candidate "Modules")) {
                return $candidate
            }
        }
    }

    $common = @(
        "D:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord",
        "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord"
    )
    foreach ($candidate in $common) {
        if (Test-Path -LiteralPath (Join-Path $candidate "Modules")) {
            return $candidate
        }
    }

    return $null
}

$game = Find-BannerlordDir $GameDir
if (-not $game) {
    throw "Could not find Bannerlord. Set BANNERLORD_GAME_DIR or create BannerlordPath.txt in the repo root."
}

Write-Host "Building CalradiaReputation ($Configuration)"
dotnet build (Join-Path $repoRoot "CalradiaReputation.sln") -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "Build failed."
}

$source = Join-Path $repoRoot "Module\CalradiaReputation"
$destination = Join-Path $game "Modules\CalradiaReputation"
if (-not (Test-Path (Join-Path $source "SubModule.xml"))) {
    throw "Module source is missing SubModule.xml"
}

Write-Host "Deploying to $destination"
New-Item -ItemType Directory -Path $destination -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $source "SubModule.xml") -Destination (Join-Path $destination "SubModule.xml") -Force
$binSource = Join-Path $source "bin"
if (Test-Path $binSource) {
    Copy-Item -LiteralPath $binSource -Destination (Join-Path $destination "bin") -Recurse -Force
}

Write-Host "Deployed CalradiaReputation to $destination"
Write-Host "Enable the module in the Bannerlord launcher. In-game, ask a companion: What do the troops think of me?"
