param(
    [string]$Configuration='Release',
    [switch]$Installer,
    [string]$DotNetExe='dotnet'
)
$ErrorActionPreference='Stop'
$projectRoot=Split-Path $PSScriptRoot -Parent
Push-Location $projectRoot
try {
    if(-not (Test-Path $DotNetExe) -and $DotNetExe -ne 'dotnet') {
        throw "dotnet executable not found: $DotNetExe"
    }

    Write-Host "Using .NET: $DotNetExe"
    & $DotNetExe --version
    if($LASTEXITCODE -ne 0){throw '.NET SDK is not usable'}

    & $DotNetExe restore companion/Asterion.Companion --configfile NuGet.Config
    if($LASTEXITCODE -ne 0){throw 'Restore failed'}

    & $DotNetExe run --project tests -- Mappings/4.10.json
    if($LASTEXITCODE -ne 0){throw 'Tests failed'}

    & $DotNetExe publish companion/Asterion.Companion -c $Configuration -r win-x64 --self-contained true -p:PublishReadyToRun=false -o release/Companion
    if($LASTEXITCODE -ne 0){throw 'Publish failed'}

    $widgetOut=Join-Path $projectRoot 'asterion-star-citizen-cockpit-v0.3.0-design.2.icuewidget'
    if(Get-Command icuewidget -ErrorAction SilentlyContinue){
        icuewidget validate widget
        if($LASTEXITCODE -ne 0){throw 'Widget validation failed'}
        icuewidget package widget
        if($LASTEXITCODE -ne 0){throw 'Widget packaging failed'}
        $generated=Get-ChildItem -File -Filter '*.icuewidget' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if($generated.FullName -ne $widgetOut){Copy-Item $generated.FullName $widgetOut -Force}
    } else {
        # .icuewidget is a ZIP container. Keep widget files at archive root.
        $temp=Join-Path $env:TEMP ('asterion-widget-'+[guid]::NewGuid())
        New-Item -ItemType Directory -Path $temp | Out-Null
        Copy-Item 'widget\*' $temp -Recurse
        $zip=$widgetOut+'.zip'
        if(Test-Path $zip){Remove-Item $zip -Force}
        if(Test-Path $widgetOut){Remove-Item $widgetOut -Force}
        Compress-Archive -Path (Join-Path $temp '*') -DestinationPath $zip -CompressionLevel Optimal
        Move-Item $zip $widgetOut
        Remove-Item $temp -Recurse -Force
    }

    if($Installer){
        $iscc=(Get-Command iscc -ErrorAction SilentlyContinue).Source
        if(-not $iscc){
            $candidates=@(
                (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
                (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe'),
                (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe')
            ) | Where-Object {$_ -and (Test-Path $_)}
            $iscc=$candidates | Select-Object -First 1
        }
        if(-not $iscc){throw 'Inno Setup compiler (ISCC.exe) is required for the installer build'}
        & $iscc installer/companion.iss
        if($LASTEXITCODE -ne 0){throw 'Installer build failed'}
    }
} finally {
    Pop-Location
}
