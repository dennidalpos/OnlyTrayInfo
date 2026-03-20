#
# OnlyTrayInfo
# Copyright (c) 2026 Danny Perondi. All rights reserved.
# Proprietary and confidential.
# Unauthorized copying, modification, distribution, disclosure, or use is prohibited.
#

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$rootDir = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent
$buildScript = Join-Path $rootDir "scripts\build.ps1"
$cleanScript = Join-Path $rootDir "scripts\clean.ps1"
$outputExe = Join-Path $rootDir "build\Release\OnlyTrayInfo.exe"

Write-Host "=== Verifying OnlyTrayInfo ===" -ForegroundColor Cyan

& $cleanScript
& $buildScript

if (-not (Test-Path -Path $outputExe -PathType Leaf)) {
    throw "Expected build output not found: $outputExe"
}

$versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($outputExe)

if ($versionInfo.ProductName -ne "OnlyTrayInfo") {
    throw "Unexpected ProductName: $($versionInfo.ProductName)"
}

if ($versionInfo.InternalName -ne "OnlyTrayInfo.exe") {
    throw "Unexpected InternalName: $($versionInfo.InternalName)"
}

if ([string]::IsNullOrWhiteSpace($versionInfo.FileVersion)) {
    throw "FileVersion is missing on $outputExe"
}

Write-Host ""
Write-Host "Verification completed successfully." -ForegroundColor Green
Write-Host "   Output: $outputExe" -ForegroundColor Cyan
Write-Host "   FileVersion: $($versionInfo.FileVersion)" -ForegroundColor Cyan
