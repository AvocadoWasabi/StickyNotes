param(
    [string]$InstallDirectory,
    [string]$StartMenuDirectory,
    [switch]$Preview
)
$ErrorActionPreference = 'Stop'
$appName = if ($Preview) { 'Markdown Sticky Notes - Tasks Preview' } else { 'Markdown Sticky Notes' }
$appFolder = if ($Preview) { 'MarkdownStickyNotes-TasksPreview' } else { 'MarkdownStickyNotes' }
if (-not $InstallDirectory) { $InstallDirectory = Join-Path $env:LOCALAPPDATA "Programs/$appFolder" }
if (-not $StartMenuDirectory) { $StartMenuDirectory = Join-Path ([Environment]::GetFolderPath('Programs')) $appName }
$sourceDirectory = [IO.Path]::GetFullPath($PSScriptRoot)
$targetDirectory = [IO.Path]::GetFullPath($InstallDirectory)
$sourceExe = Join-Path $sourceDirectory 'StickyNotes.exe'
if (-not (Test-Path -LiteralPath $sourceExe)) { throw 'Extract the complete ZIP before running Install.cmd.' }
$targetExe = Join-Path $targetDirectory 'StickyNotes.exe'
$running = Get-Process StickyNotes -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $targetExe }
if ($running) { throw 'Exit Sticky Notes from the notification area, then run Install.cmd again.' }
New-Item -ItemType Directory -Path $targetDirectory -Force | Out-Null
if ($sourceDirectory.TrimEnd('\') -ne $targetDirectory.TrimEnd('\')) {
    Get-ChildItem -LiteralPath $sourceDirectory | Copy-Item -Destination $targetDirectory -Recurse -Force
}
New-Item -ItemType Directory -Path $StartMenuDirectory -Force | Out-Null
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut((Join-Path $StartMenuDirectory "$appName.lnk"))
$shortcut.TargetPath = $targetExe
$shortcut.WorkingDirectory = $targetDirectory
$shortcut.IconLocation = "$targetExe,0"
$shortcut.Save()
Write-Host "Installed. Open $appName from the Start menu."
Write-Host "Location: $targetDirectory"
