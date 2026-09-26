param([switch]$SkipTests)
$ErrorActionPreference='Stop'
Push-Location $PSScriptRoot
try {
    dotnet build Rhino2Straus.sln -c Release --nologo
    if($LASTEXITCODE -ne 0){throw 'Build failed.'}
    if(-not $SkipTests){
        dotnet run --project Rhino2Straus.Tests -c Release --no-build
        if($LASTEXITCODE -ne 0){throw 'Managed tests failed.'}
    }
    $output=Join-Path $PSScriptRoot 'artifacts/Rhino2Straus'
    New-Item -ItemType Directory -Path $output -Force | Out-Null
    $source=Join-Path $PSScriptRoot 'Rhino2Straus.Grasshopper/bin/Release/net8.0-windows'
    $files=@('Rhino2Straus.Grasshopper.gha','Rhino2Straus.Core.dll','Rhino2Straus.Api.dll','Rhino2Straus.Import.dll','Rhino2Straus.Grasshopper.Types.dll','GrasshopperHelper.dll')
    foreach($file in $files){Copy-Item -LiteralPath (Join-Path $source $file) -Destination (Join-Path $output $file) -Force}
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md') -Destination $output -Force
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'docs') -Destination $output -Recurse -Force
    $archive=Join-Path $PSScriptRoot 'artifacts/Rhino2Straus-0.4.1.zip'
    Compress-Archive -Path (Join-Path $output '*') -DestinationPath $archive -Force
    Get-FileHash -LiteralPath $archive -Algorithm SHA256
    Write-Output "Package: $archive"
} finally { Pop-Location }
