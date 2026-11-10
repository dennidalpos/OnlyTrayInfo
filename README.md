# OnlyTrayInfo

Small WinForms app that lives in the Windows notification area and shows quick information about the PC: host, network, printers and user. The main window offers a detailed summary and a few quick actions such as launching **Quick Assist** and setting the default printer. The interface is in Italian.

## Features

- **Tray icon** with a dynamic tooltip (hostname and primary IP address, limited to 63 characters by Windows).
- **System dashboard**:
  - hostname, user and DNS domain;
  - network interfaces with status, speed, MAC, IPv4, gateway and DNS (loopback and tunnel adapters are filtered out);
  - installed printers, with the default one marked;
  - an internal in-memory error log shown in the "LOG ERRORI" section.
- **Quick Assist**: opens Quick Assist through the `ms-quick-assist:` protocol, falling back to `quickassist.exe` in `%WINDIR%\System32`, `%WINDIR%\SysWOW64` and `PATH`.
- **Printer management**: pick a printer from the list and set it as the system default.
- **Start in tray**: the window stays hidden and reopens from the tray menu.

## Requirements

- Windows with WinForms support.
- .NET Framework 4.0 (project target).
- Access to the Windows network and printing APIs.

## Build

The project defaults to `build/Debug/` or `build/Release/`. For a direct build without invoking cleanup, use the installed Framework MSBuild:

```powershell
$msbuild = Join-Path $env:WINDIR 'Microsoft.NET/Framework/v4.0.30319/MSBuild.exe'
& $msbuild src/OnlyTrayInfo/OnlyTrayInfo.csproj /p:Configuration=Release /nologo
```

For isolated agent checks, use a fresh central-runtime checkout and redirect `OutputPath`, `IntermediateOutputPath`, TEMP and TMP there. A direct build checks compilation; inspect output product metadata separately.

```powershell
# Build Release (build\Release\OnlyTrayInfo.exe, with a timestamped informational version)
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1

# Destructive helper: calls clean before build; review scope before use
powershell -ExecutionPolicy Bypass -File .\scripts\verify.ps1

# Cleanup only after separately authorizing exact generated paths
powershell -ExecutionPolicy Bypass -File .\scripts\clean.ps1
```

With Visual Studio, open `src/OnlyTrayInfo/OnlyTrayInfo.csproj` and build. With MSBuild:

```bat
cd src\OnlyTrayInfo
msbuild OnlyTrayInfo.csproj /p:Configuration=Release
```

There is no unit-test suite. `verify.ps1` is a metadata smoke helper that calls cleanup; it does not qualify the tray UI, Quick Assist or printer behavior. Keep existing outputs, logs and evidence until their exact removal is authorized. No packaging/publish flow is defined. See the [English specification](PROJECT_SPEC.md).

## Usage

1. Run `OnlyTrayInfo.exe` from `build\Release\`; it starts minimized in the tray.
2. Double-click the icon or use the context menu **Apri** to show the window; **Esci** closes the app.
3. In the window, **Assistenza rapida** starts Quick Assist, and the printer list with **Imposta predefinita** sets the default printer.

## Troubleshooting

- **No IP or network information**: the network interface must be up; loopback and tunnel adapters are ignored.
- **Cannot set the default printer**: check that the printer is installed correctly, that the app has the needed permissions and that the printer is selected in the list.
- **Quick Assist does not open**: install or enable **Quick Assist** in Windows.

## Project structure

```text
scripts/
  build.ps1  clean.ps1  verify.ps1
src/OnlyTrayInfo/
  OnlyTrayInfo.csproj   # .NET Framework 4.0 project
  Program.cs            # UI, main logic and system information gathering
  app.manifest
  Properties/AssemblyInfo.cs
```

## Security

The app reads local system information and explicitly changes the chosen default printer. Quick Assist is launched as a separate Windows application; its connections are outside this utility's local inventory workflow. See [SECURITY.md](SECURITY.md) to report a vulnerability.

## License

Proprietary and confidential, all rights reserved. The repository may be viewed for evaluation by authorized parties only; reuse, copying, modification, distribution, sublicensing and commercial use require prior written permission. See [LICENSE](LICENSE). Copyright (c) 2026 Danny Perondi.
