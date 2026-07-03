#requires -version 5
# Rebuild the CYD firmware + filesystem and refresh the web-flasher bins under docs/flash/.
# The web flasher (docs/index.html) uses esp-web-tools with docs/flash/manifest.json, which
# writes four parts to a single-app (no-OTA) ESP32 layout:
#   cyd28/bootloader.bin @ 0x1000
#   cyd28/partitions.bin @ 0x8000
#   cyd28/firmware.bin   @ 0x10000
#   cyd28/littlefs.bin   @ 0x2E0000   (presets + audio catalog ship with the install)
#
# Usage:  pwsh -File scripts/refresh-flasher.ps1
# Then:   git add docs/flash; git commit
param(
  [string]$pio = "$env:USERPROFILE\.platformio\penv\Scripts\platformio.exe"
)
$ErrorActionPreference = "Stop"
$env:PYTHONIOENCODING = "utf-8"
$root  = Resolve-Path (Join-Path $PSScriptRoot "..")
$env   = "cyd28_ili9341"
$build = Join-Path $root ".pio\build\$env"
$dst   = Join-Path $root "docs\flash\cyd28"

Write-Host "==> building firmware + filesystem ($env)"
& $pio run -e $env
if ($LASTEXITCODE -ne 0) { throw "firmware build failed" }
& $pio run -e $env -t buildfs
if ($LASTEXITCODE -ne 0) { throw "filesystem build failed" }

if (-not (Test-Path $dst)) { New-Item -ItemType Directory -Force $dst | Out-Null }
foreach ($b in "bootloader.bin","partitions.bin","firmware.bin","littlefs.bin") {
  Copy-Item (Join-Path $build $b) (Join-Path $dst $b) -Force
  Write-Host ("  {0,-16} {1,8} bytes" -f $b, (Get-Item (Join-Path $dst $b)).Length)
}
Write-Host "`nRefreshed docs/flash/cyd28. Next: git add docs/flash; git commit"
