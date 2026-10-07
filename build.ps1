param([switch]$Publish, [switch]$TasksPreview)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$localSdk = Join-Path $PSScriptRoot '.tools\dotnet\dotnet.exe'
$dotnet = if (Test-Path $localSdk) { $localSdk } else { (Get-Command dotnet -ErrorAction Stop).Source }
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_CLI_HOME = Join-Path $PSScriptRoot '.tools\cli'
$env:NUGET_PACKAGES = Join-Path $PSScriptRoot '.tools\nuget'
& $dotnet restore StickyNotes.sln --configfile NuGet.Config
if ($LASTEXITCODE -ne 0) { throw 'Restore failed' }
$flavor = "-p:TasksPreviewBuild=$($TasksPreview.IsPresent.ToString().ToLowerInvariant())"
& $dotnet build StickyNotes.sln -c Release --no-restore $flavor
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
& $dotnet run --project tests/StickyNotes.Tests -c Release --no-build --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }
$appOutput = if ($TasksPreview) { 'artifact/tasks-cli-preview/app' } else { 'artifacts/app' }
& $dotnet publish src/StickyNotes/StickyNotes.csproj -c Release -r win-x64 --self-contained true -o $appOutput --configfile NuGet.Config $flavor
if ($LASTEXITCODE -ne 0) { throw 'Publish failed' }
Copy-Item README.md,README.ja.md,README.zh-CN.md,CHANGELOG.md,CHANGELOG.ja.md,CHANGELOG.zh-CN.md,THIRD-PARTY-NOTICES.txt -Destination $appOutput
Copy-Item examples -Destination $appOutput -Recurse -Force
Copy-Item docs -Destination $appOutput -Recurse -Force
Copy-Item packaging/Install.cmd,packaging/Install.ps1 -Destination $appOutput
if ($TasksPreview) { Copy-Item packaging/tasks-preview/Install.cmd -Destination $appOutput -Force }
New-Item -ItemType Directory -Force (Join-Path $appOutput 'licenses') | Out-Null
$runtime = Get-ChildItem (Join-Path $env:NUGET_PACKAGES 'microsoft.netcore.app.runtime.win-x64') -Directory | Sort-Object Name -Descending | Select-Object -First 1
$desktop = Get-ChildItem (Join-Path $env:NUGET_PACKAGES 'microsoft.windowsdesktop.app.runtime.win-x64') -Directory | Sort-Object Name -Descending | Select-Object -First 1
Copy-Item (Join-Path $runtime.FullName 'LICENSE.TXT') -Destination (Join-Path $appOutput 'licenses/dotnet-LICENSE.txt')
Copy-Item (Join-Path $runtime.FullName 'THIRD-PARTY-NOTICES.TXT') -Destination (Join-Path $appOutput 'licenses/dotnet-THIRD-PARTY-NOTICES.txt')
Copy-Item (Join-Path $desktop.FullName 'LICENSE') -Destination (Join-Path $appOutput 'licenses/windowsdesktop-LICENSE.txt')
if ($Publish) {
    $archiveName = if ($TasksPreview) { 'artifact/tasks-cli-preview/StickyNotes-TasksPreview-win-x64.zip' } else { 'artifacts/StickyNotes-win-x64.zip' }
    $archive = Join-Path $PSScriptRoot $archiveName
    Compress-Archive -Path (Join-Path $PSScriptRoot "$appOutput/*") -DestinationPath $archive -Force
    $hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
    [IO.File]::WriteAllText("$archive.sha256", "$hash  $([IO.Path]::GetFileName($archive))`n", [Text.Encoding]::ASCII)
}
Write-Host "Install: $appOutput/Install.cmd"
Write-Host "Run: $appOutput/StickyNotes.exe"
