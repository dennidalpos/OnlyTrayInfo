# Product specification

[Retained Italian source](PROJECT_SPEC.it.md)

OnlyTrayInfo is a small Windows notification-area utility for viewing local host, user/domain, network and printer information. It can launch Quick Assist and change the default printer through explicit UI actions.

## Architecture and behavior

The project targets .NET Framework 4.0/WinForms. src/OnlyTrayInfo contains one C# project. Program.cs includes bootstrap, main form, system-information collection and UI logic.

Startup hides the window in the tray. Its tooltip contains hostname and primary IPv4, bounded to Windows' 63-character tooltip limit. The main window presents host/network/printer details and an in-memory error log. Users reopen it through the tray and explicitly choose Quick Assist or a default printer.

## Build and qualification boundaries

scripts/build.ps1 builds a temporary copy with a timestamped informational version and writes build/Release. Its implementation removes temporary/generated files. verify.ps1 calls clean before build and checks product metadata; it is not a comprehensive automated UI/system test. No installer/publish pipeline is defined.

Inspect and authorize the actual cleanup scope before running these scripts on an existing checkout. For non-destructive qualification, build a fresh runtime fixture directly with Framework MSBuild and redirect output/intermediates.

The application reads Windows system/network APIs and can alter the selected default printer. Quick Assist is an external Windows application with separate behavior; launching it does not prove a remote support connection or that the target machine has it installed.

Current source, checks and documentation must agree. Proposed tracker corrections remain in the dossier review; no task closes solely from a successful build.
