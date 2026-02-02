param(
  [string]$ProjectName = "TrayPcInfo"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$rootDir = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent
$projectDir = Join-Path $rootDir "src\TrayPcInfo"
$buildDir = Join-Path $rootDir "build"
$tempDir = Join-Path $rootDir "temp_build"

New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

$BaseVersion = '1.0'
$BuildStamp = Get-Date -Format 'yyyyMMdd.HHmm'
$FullVersion = "$BaseVersion.$BuildStamp"

$assemblyInfoPath = Join-Path $projectDir "Properties\AssemblyInfo.cs"
$assemblyInfoContent = Get-Content $assemblyInfoPath -Raw
$assemblyInfoContent = $assemblyInfoContent -replace 'AssemblyInformationalVersion\(".*?"\)', "AssemblyInformationalVersion(`"$FullVersion`")"
Set-Content -Path (Join-Path $tempDir "AssemblyInfo.cs") -Value $assemblyInfoContent -Encoding UTF8

$programCsPath = Join-Path $projectDir "Program.cs"
Copy-Item -Path $programCsPath -Destination (Join-Path $tempDir "Program.cs") -Force

$csc = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    $csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
}
if (-not (Test-Path $csc)) {
    throw "C# compiler (csc.exe) not found"
}

$outputExe = Join-Path $tempDir "$ProjectName.exe"
$assemblyInfoFile = Join-Path $tempDir "AssemblyInfo.cs"
$programFile = Join-Path $tempDir "Program.cs"
$manifestFile = Join-Path $projectDir "app.manifest"

Write-Host ">> Building with csc.exe"
Write-Host "   Version: $FullVersion"

$cscArgs = @(
    "/target:winexe",
    "/out:$outputExe",
    "/optimize+",
    "/platform:anycpu",
    "/reference:System.dll",
    "/reference:System.Core.dll",
    "/reference:System.Drawing.dll",
    "/reference:System.Windows.Forms.dll",
    "/win32manifest:$manifestFile",
    "/nologo",
    "/nowarn:1701,1702",
    $assemblyInfoFile,
    $programFile
)

& $csc $cscArgs

if ($LASTEXITCODE -ne 0) {
    throw "Build failed with code $LASTEXITCODE"
}

if (-not (Test-Path -Path $outputExe -PathType Leaf)) {
    throw "Executable not generated: $outputExe"
}

New-Item -ItemType Directory -Force -Path $buildDir | Out-Null
$finalExe = Join-Path $buildDir "$ProjectName.exe"
Copy-Item -Path $outputExe -Destination $finalExe -Force

Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "✓ Build completed successfully!" -ForegroundColor Green
Write-Host "   File: $finalExe" -ForegroundColor Cyan
Write-Host "   Version: $FullVersion" -ForegroundColor Cyan
$fileSize = (Get-Item $finalExe).Length / 1KB
Write-Host "   Size: $($fileSize.ToString('0.0')) KB" -ForegroundColor Cyan
Write-Host ""
Write-Host "To clean generated files, run: .\scripts\clean.ps1" -ForegroundColor Gray
