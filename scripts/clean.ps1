#
# OnlyTrayInfo
# Copyright (c) 2026 Danny Perondi. All rights reserved.
# Proprietary and confidential.
# Unauthorized copying, modification, distribution, disclosure, or use is prohibited.
#

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Cleaning Project ===" -ForegroundColor Cyan
Write-Host ""

$rootDir = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent
$itemsRemoved = 0

function Remove-GeneratedDirectory {
    param(
        [string]$Path,
        [string]$Label,
        [switch]$FailOnError
    )

    if (-not (Test-Path $Path)) {
        Write-Host "$Label folder not found" -ForegroundColor Gray
        return
    }

    Write-Host "Found $Label folder: $Path" -ForegroundColor Yellow

    try {
        $fileCount = (Get-ChildItem -Path $Path -File -Recurse -ErrorAction SilentlyContinue | Measure-Object).Count
        Remove-Item -Path $Path -Recurse -Force
        Write-Host "[OK] $Label folder removed ($fileCount files)" -ForegroundColor Green
        $script:itemsRemoved += $fileCount
    }
    catch {
        Write-Host "[ERROR] Cannot remove $Label folder: $_" -ForegroundColor Red
        if ($FailOnError) {
            exit 1
        }
    }
}

Remove-GeneratedDirectory -Path (Join-Path $rootDir "build") -Label "build" -FailOnError
Remove-GeneratedDirectory -Path (Join-Path $rootDir "tmp") -Label "tmp"
Remove-GeneratedDirectory -Path (Join-Path $rootDir "temp_build") -Label "temp_build"

$artifactDirectories = Get-ChildItem -Path $rootDir -Directory -Recurse -ErrorAction SilentlyContinue |
    Where-Object {
        $_.Name -in @("bin", "obj") -and $_.FullName -notlike "*\.git\*"
    }

foreach ($directory in $artifactDirectories) {
    Remove-GeneratedDirectory -Path $directory.FullName -Label $directory.Name
}

Write-Host ""
Write-Host "Searching for temporary files..." -ForegroundColor Cyan

$tempFiles = @(
    "*.pdb",
    "*.exe.config",
    "*.cache",
    "*.log",
    "*.suo",
    "*.user"
)

foreach ($pattern in $tempFiles) {
    $files = Get-ChildItem -Path $rootDir -Filter $pattern -Recurse -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        if ($file.FullName -notlike "*\.git\*") {
            try {
                Remove-Item -Path $file.FullName -Force
                Write-Host "[OK] Removed: $($file.Name)" -ForegroundColor Green
                $itemsRemoved++
            }
            catch {
                Write-Host "[ERROR] Cannot remove: $($file.Name)" -ForegroundColor Red
            }
        }
    }
}

Write-Host ""
if ($itemsRemoved -eq 0) {
    Write-Host "No files to clean" -ForegroundColor Gray
}
else {
    Write-Host "=== Cleaning Completed ===" -ForegroundColor Green
    Write-Host "   Total items removed: $itemsRemoved" -ForegroundColor Cyan
}
