#
# OnlyTrayInfo
# Copyright (c) 2026 Danny Perondi. All rights reserved.
# Proprietary and confidential.
# Unauthorized copying, modification, distribution, disclosure, or use is prohibited.
#

param(
  [string]$ProjectName = "OnlyTrayInfo"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$rootDir = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent
$projectDir = Join-Path $rootDir "src\OnlyTrayInfo"
$buildDir = Join-Path $rootDir "build"
$releaseDir = Join-Path $buildDir "Release"
$tmpRootDir = Join-Path $rootDir "tmp"
$tempDir = Join-Path $tmpRootDir "build"
$tempProjectDir = Join-Path $tempDir "OnlyTrayInfo"
$tempObjDir = Join-Path $tempDir "obj"

if (Test-Path $tempDir) {
    Remove-Item -Path $tempDir -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $tempProjectDir | Out-Null
New-Item -ItemType Directory -Force -Path $tempObjDir | Out-Null

$BaseVersion = '1.0'
$BuildStamp = Get-Date -Format 'yyyyMMdd.HHmm'
$FullVersion = "$BaseVersion.$BuildStamp"

$assemblyInfoPath = Join-Path $tempProjectDir "Properties\AssemblyInfo.cs"
$tempProjectFile = Join-Path $tempProjectDir "OnlyTrayInfo.csproj"

Copy-Item -Path (Join-Path $projectDir "*") -Destination $tempProjectDir -Recurse -Force

$assemblyInfoContent = Get-Content $assemblyInfoPath -Raw
$assemblyInfoContent = $assemblyInfoContent -replace 'AssemblyInformationalVersion\(".*?"\)', "AssemblyInformationalVersion(`"$FullVersion`")"
Set-Content -Path $assemblyInfoPath -Value $assemblyInfoContent -Encoding UTF8

$frameworkPath = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\"
$msbuildExe = Join-Path $frameworkPath "MSBuild.exe"
if (-not (Test-Path $msbuildExe)) {
    $frameworkPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\"
    $msbuildExe = Join-Path $frameworkPath "MSBuild.exe"
}
if (-not (Test-Path $msbuildExe)) {
    throw "MSBuild.exe for .NET Framework 4.0 not found"
}

New-Item -ItemType Directory -Force -Path $releaseDir | Out-Null
$finalExe = Join-Path $releaseDir "$ProjectName.exe"
if (Test-Path $finalExe) {
    Remove-Item -Path $finalExe -Force
}

Write-Host ">> Building with MSBuild.exe"
Write-Host "   Version: $FullVersion"

$msbuildArgs = @(
    $tempProjectFile,
    "/t:Build",
    "/p:Configuration=Release",
    "/p:Platform=AnyCPU",
    "/p:OutDir=$releaseDir\",
    "/p:BaseIntermediateOutputPath=$tempObjDir\",
    "/p:IntermediateOutputPath=$tempObjDir\Release\",
    "/p:FrameworkPathOverride=$frameworkPath",
    "/nologo",
    "/verbosity:minimal"
)

& $msbuildExe $msbuildArgs

if ($LASTEXITCODE -ne 0) {
    throw "Build failed with code $LASTEXITCODE"
}

if (-not (Test-Path -Path $finalExe -PathType Leaf)) {
    throw "Executable not generated: $finalExe"
}

Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
if (Test-Path $tmpRootDir) {
    $tmpEntries = Get-ChildItem -Path $tmpRootDir -Force -ErrorAction SilentlyContinue
    if (($tmpEntries | Measure-Object).Count -eq 0) {
        Remove-Item -Path $tmpRootDir -Force -ErrorAction SilentlyContinue
    }
}

Write-Host ""
Write-Host "Build completed successfully." -ForegroundColor Green
Write-Host "   File: $finalExe" -ForegroundColor Cyan
Write-Host "   Version: $FullVersion" -ForegroundColor Cyan
$fileSize = (Get-Item $finalExe).Length / 1KB
Write-Host "   Size: $($fileSize.ToString('0.0')) KB" -ForegroundColor Cyan
Write-Host ""
Write-Host "To clean generated files, run: .\scripts\clean.ps1" -ForegroundColor Gray
