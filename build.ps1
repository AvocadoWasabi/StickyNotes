param([switch]$Publish)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$localSdk = Join-Path $PSScriptRoot '.tools\dotnet\dotnet.exe'
$dotnet = if (Test-Path $localSdk) { $localSdk } else { (Get-Command dotnet -ErrorAction Stop).Source }
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_CLI_HOME = Join-Path $PSScriptRoot '.tools\cli'
$env:NUGET_PACKAGES = Join-Path $PSScriptRoot '.tools\nuget'
& $dotnet restore StickyNotes.sln --configfile NuGet.Config
if ($LASTEXITCODE -ne 0) { throw 'Restore failed' }
& $dotnet build StickyNotes.sln -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
& $dotnet run --project tests/StickyNotes.Tests -c Release --no-build --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }
if ($Publish) {
    & $dotnet publish src/StickyNotes/StickyNotes.csproj -c Release -r win-x64 --self-contained true -o artifacts/app --configfile NuGet.Config
    if ($LASTEXITCODE -ne 0) { throw 'Publish failed' }
    Copy-Item README.md,README.ja.md,THIRD-PARTY-NOTICES.txt -Destination artifacts/app
    Copy-Item examples -Destination artifacts/app -Recurse -Force
    Copy-Item docs -Destination artifacts/app -Recurse -Force
    Copy-Item packaging/Install.cmd,packaging/Install.ps1 -Destination artifacts/app
    New-Item -ItemType Directory -Force artifacts/app/licenses | Out-Null
    $runtime = Get-ChildItem (Join-Path $env:NUGET_PACKAGES 'microsoft.netcore.app.runtime.win-x64') -Directory | Sort-Object Name -Descending | Select-Object -First 1
    $desktop = Get-ChildItem (Join-Path $env:NUGET_PACKAGES 'microsoft.windowsdesktop.app.runtime.win-x64') -Directory | Sort-Object Name -Descending | Select-Object -First 1
    Copy-Item (Join-Path $runtime.FullName 'LICENSE.TXT') -Destination artifacts/app/licenses/dotnet-LICENSE.txt
    Copy-Item (Join-Path $runtime.FullName 'THIRD-PARTY-NOTICES.TXT') -Destination artifacts/app/licenses/dotnet-THIRD-PARTY-NOTICES.txt
    Copy-Item (Join-Path $desktop.FullName 'LICENSE') -Destination artifacts/app/licenses/windowsdesktop-LICENSE.txt
    $archive = Join-Path $PSScriptRoot 'artifacts/StickyNotes-win-x64.zip'
    Compress-Archive -Path (Join-Path $PSScriptRoot 'artifacts/app/*') -DestinationPath $archive -Force
    $hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
    [IO.File]::WriteAllText("$archive.sha256", "$hash  StickyNotes-win-x64.zip`n", [Text.Encoding]::ASCII)
    Write-Host '起動: artifacts\app\StickyNotes.exe（配布時はappフォルダ全体をコピー）'
}
