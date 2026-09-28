param([string]$Version = ((Get-Content "$PSScriptRoot/../widget/manifest.json" -Raw | ConvertFrom-Json).version))
$ErrorActionPreference='Stop'
$projectRoot=Split-Path $PSScriptRoot -Parent
$portableRoot=Join-Path $projectRoot 'release/Companion'
if(-not(Test-Path (Join-Path $portableRoot 'SC-Xeneon-Companion.exe'))){throw 'Build the companion before packaging'}
$launch=@'
@echo off
cd /d "%~dp0"
start "" "SC-Xeneon-Companion.exe" --portable --background
timeout /t 3 /nobreak >nul
start "" "http://127.0.0.1:32147/"
'@
[IO.File]::WriteAllText((Join-Path $portableRoot 'Ouvrir-Asterion.cmd'),$launch,[Text.Encoding]::ASCII)
[IO.File]::WriteAllText((Join-Path $portableRoot 'Simulation.cmd'),$launch.Replace('--portable --background','--portable --simulate --background'),[Text.Encoding]::ASCII)
Copy-Item (Join-Path $projectRoot "asterion-star-citizen-cockpit-v$Version.icuewidget") $portableRoot -Force
Copy-Item (Join-Path $projectRoot 'docs/portable.md') (Join-Path $portableRoot 'LIRE-MOI.md') -Force
Compress-Archive -Path (Join-Path $portableRoot '*') -DestinationPath (Join-Path $projectRoot "release/AsterionEdge-portable-v$Version.zip") -CompressionLevel Optimal -Force
