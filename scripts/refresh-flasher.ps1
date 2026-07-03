#requires -version 5
# Rebuild the CYD firmware (+ filesystem) for every variant and refresh the web-flasher bins
# under docs/flash/. The web flasher (docs/index.html) uses esp-web-tools with a per-variant
# manifest, writing to a single-app (no-OTA) ESP32 layout:
#   cyd28/bootloader.bin @ 0x1000     (shared)
#   cyd28/partitions.bin @ 0x8000     (shared)
#   cyd28/littlefs.bin   @ 0x2E0000   (shared: presets + audio catalog)
#   <variant>/firmware.bin @ 0x10000  (per variant)
# Variants:
#   cyd28_ili9341 -> flash/manifest.json         (CYD2USB, standard ESP32-2432S028R)
#   cyd28_elegoo  -> flash/manifest-elegoo.json  (Elegoo USB-C, portrait-native panel)
#
# Usage:  pwsh -File scripts/refresh-flasher.ps1
# Then:   git add docs/flash; git commit
param(
  [string]$pio = "$env:USERPROFILE\.platformio\penv\Scripts\platformio.exe"
)
$ErrorActionPreference = "Stop"
$env:PYTHONIOENCODING = "utf-8"
$root  = Resolve-Path (Join-Path $PSScriptRoot "..")
$flash = Join-Path $root "docs\flash"

function Build([string]$e) {
  Write-Host "==> building $e"
  & $pio run -e $e; if ($LASTEXITCODE -ne 0) { throw "firmware build failed: $e" }
}

# --- standard CYD2USB: firmware + the shared bootloader/partitions/filesystem ---
Build "cyd28_ili9341"
& $pio run -e cyd28_ili9341 -t buildfs; if ($LASTEXITCODE -ne 0) { throw "fs build failed" }
$std = Join-Path $root ".pio\build\cyd28_ili9341"
New-Item -ItemType Directory -Force (Join-Path $flash "cyd28") | Out-Null
foreach ($b in "bootloader.bin","partitions.bin","firmware.bin","littlefs.bin") {
  Copy-Item (Join-Path $std $b) (Join-Path $flash "cyd28\$b") -Force
}

# --- Elegoo variant: only its firmware differs (bootloader/partitions/fs are identical) ---
Build "cyd28_elegoo"
New-Item -ItemType Directory -Force (Join-Path $flash "elegoo") | Out-Null
Copy-Item (Join-Path $root ".pio\build\cyd28_elegoo\firmware.bin") (Join-Path $flash "elegoo\firmware.bin") -Force

Write-Host "`n--- refreshed flasher bins ---"
Write-Host ("cyd28/firmware.bin  = {0} bytes" -f (Get-Item (Join-Path $flash 'cyd28\firmware.bin')).Length)
Write-Host ("elegoo/firmware.bin = {0} bytes" -f (Get-Item (Join-Path $flash 'elegoo\firmware.bin')).Length)
Write-Host "`nNext: git add docs/flash; git commit"
