# Security Policy

## Supported versions

Only the latest commit on the `main` branch is supported.

## Reporting a vulnerability

Please do not open a public issue for security problems.

Use GitHub's private vulnerability reporting: open the **Security** tab of this repository and choose **Report a vulnerability**. Include the reproduction steps and the impact you observed.

You can expect an acknowledgement within a few days. Confirmed issues are fixed on `main` and disclosed after a fix is available.

## Scope notes

- The app reads local system information (hostname, user, network interfaces, printers) and can change the default printer; it has no network component.
- Dependencies are monitored with Dependabot and secret scanning with push protection.
