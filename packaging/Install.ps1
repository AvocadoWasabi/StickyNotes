param(
    [string]$InstallDirectory = (Join-Path $env:LOCALAPPDATA 'Programs/MarkdownStickyNotes'),
    [string]$StartMenuDirectory = (Join-Path ([Environment]::GetFolderPath('Programs')) 'Markdown Sticky Notes')
)
$ErrorActionPreference = 'Stop'
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
$shortcut = $shell.CreateShortcut((Join-Path $StartMenuDirectory 'Markdown Sticky Notes.lnk'))
$shortcut.TargetPath = $targetExe
$shortcut.WorkingDirectory = $targetDirectory
$shortcut.IconLocation = "$targetExe,0"
$shortcut.Save()
Write-Host 'Installed. Open Markdown Sticky Notes from the Start menu.'
Write-Host "Location: $targetDirectory"
