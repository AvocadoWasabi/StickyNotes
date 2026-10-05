# Creates isolated sample data; does not launch the app or change personal settings.
param([ValidateSet('ja', 'en')][string]$Language = 'ja')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$data = Join-Path $root ('artifacts/screenshot-demo-' + [Guid]::NewGuid().ToString('N'))
$notes = Join-Path $data 'notes'
$daily = Join-Path $data 'daily'
New-Item -ItemType Directory -Path $notes,$daily -Force | Out-Null
$samples = Join-Path $root 'examples/screenshots'
if ($Language -eq 'en') { $samples = Join-Path $samples 'en' }
Copy-Item (Join-Path $samples 'Tasks.md'),(Join-Path $samples 'Project.md') -Destination $notes
Copy-Item (Join-Path $samples 'Daily.md') -Destination (Join-Path $daily ((Get-Date -Format 'yyyy-MM-dd') + '.md'))
$windows = @(
    @{ Path = (Join-Path $notes 'Tasks.md'); Width = 460; Height = 580; PixelLeft = 100; PixelTop = 100; HasPixelPosition = $true; Color = 'yellow' },
    @{ Path = (Join-Path $notes 'Project.md'); Width = 460; Height = 580; PixelLeft = 600; PixelTop = 100; HasPixelPosition = $true; Color = 'blue' },
    @{ Path = ''; Heading = 'Tasks'; Daily = $true; Width = 460; Height = 580; PixelLeft = 1100; PixelTop = 100; HasPixelPosition = $true; Color = 'green' }
)
@{ NotesFolder = $notes; DailyFolder = $daily; DailyPattern = 'yyyy-MM-dd'; Windows = $windows } |
    ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $data 'settings.json') -Encoding utf8
Write-Output $data
