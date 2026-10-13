# Builds the mod and stages the files to upload into workshop/content.
# Upload afterwards with Mega Crit's ModUploader (see README.md):
#   ModUploader.exe upload -w <path to this workshop folder>
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$content = Join-Path $PSScriptRoot 'content'

dotnet publish (Join-Path $root 'ScalingMod.csproj') -c ExportRelease
if ($LASTEXITCODE -ne 0) { throw 'Build failed. Is the game still running?' }

# The build copies the release files to the game's mods folder; take them from there.
$steam = (Get-ItemProperty 'HKCU:\Software\Valve\Steam').SteamPath
$built = Join-Path $steam 'steamapps/common/Slay the Spire 2/mods/ScalingMod'

if (Test-Path $content) { Remove-Item $content -Recurse -Force }
New-Item -ItemType Directory $content | Out-Null
foreach ($file in 'ScalingMod.dll', 'ScalingMod.json', 'ScalingMod.pck') {
    Copy-Item (Join-Path $built $file) $content
}

Get-ChildItem $content | Format-Table Name, Length, LastWriteTime
Write-Host "Staged. Version: $((Get-Content (Join-Path $content 'ScalingMod.json') | ConvertFrom-Json).version)"
