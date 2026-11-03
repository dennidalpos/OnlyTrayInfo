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

The repository writes build output to `build/Debug/` and `build/Release/`; `tmp/` holds temporary files removed by the scripts.

```powershell
# Build Release (build\Release\OnlyTrayInfo.exe, with a timestamped informational version)
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1

# Smoke check: clean, build, and verify the executable's product metadata
powershell -ExecutionPolicy Bypass -File .\scripts\verify.ps1

# Remove build/, tmp/, bin/ and obj/
powershell -ExecutionPolicy Bypass -File .\scripts\clean.ps1
```

With Visual Studio, open `src/OnlyTrayInfo/OnlyTrayInfo.csproj` and build. With MSBuild:

```bat
cd src\OnlyTrayInfo
msbuild OnlyTrayInfo.csproj /p:Configuration=Release
```

There is no automated test suite; `verify.ps1` is the repeatable check. The repository defines no packaging or publish flow.

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

The app only reads local system information and does not send data over the network. See [SECURITY.md](SECURITY.md) to report a vulnerability.

## License

Proprietary and confidential, all rights reserved. The repository may be viewed for evaluation by authorized parties only; reuse, copying, modification, distribution, sublicensing and commercial use require prior written permission. See [LICENSE](LICENSE). Copyright (c) 2026 Danny Perondi.
