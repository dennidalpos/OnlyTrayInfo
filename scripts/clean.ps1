Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Cleaning Project ===" -ForegroundColor Cyan
Write-Host ""

$rootDir = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent
$buildPath = Join-Path $rootDir "build"
$tempBuildPath = Join-Path $rootDir "temp_build"

$itemsRemoved = 0

if (Test-Path $buildPath) {
    Write-Host "Found build folder: $buildPath" -ForegroundColor Yellow

    try {
        $fileCount = (Get-ChildItem -Path $buildPath -File -Recurse | Measure-Object).Count
        Remove-Item -Path $buildPath -Recurse -Force
        Write-Host "[OK] Build folder removed ($fileCount files)" -ForegroundColor Green
        $itemsRemoved += $fileCount
    }
    catch {
        Write-Host "[ERROR] Cannot remove build folder: $_" -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "Build folder not found" -ForegroundColor Gray
}

if (Test-Path $tempBuildPath) {
    Write-Host "Found temp_build folder: $tempBuildPath" -ForegroundColor Yellow

    try {
        $fileCount = (Get-ChildItem -Path $tempBuildPath -File -Recurse | Measure-Object).Count
        Remove-Item -Path $tempBuildPath -Recurse -Force
        Write-Host "[OK] Temp build folder removed ($fileCount files)" -ForegroundColor Green
        $itemsRemoved += $fileCount
    }
    catch {
        Write-Host "[ERROR] Cannot remove temp_build folder: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Searching for temporary files..." -ForegroundColor Cyan

$tempFiles = @(
    "*.pdb",
    "*.exe.config",
    "*.cache",
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
