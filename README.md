# Antigravity Proxy Launcher for Windows

A one-click Windows launcher for [Antigravity-Proxy](https://github.com/yuaotian/antigravity-proxy). It restores Antigravity's proxy DLL after an application update, without enabling Clash/Mihomo TUN mode.

## What it does

- Finds the installed `Antigravity.exe` automatically.
- Stops existing Antigravity processes before replacing files.
- Backs up the current `version.dll` and `config.json`.
- Reads the local Clash Verge `mixed-port`, `socks-port`, or `port` and updates the proxy configuration.
- Copies the matching x64 payload and launches Antigravity.
- Can be run again after every Antigravity update.

Only Antigravity-related processes are handled. Other system traffic is unchanged.

## Quick start

1. Keep Clash Verge running with a local SOCKS5 or mixed port.
2. Download `AntigravityProxyLauncher.exe` together with the `payload` folder from a release, or build it locally.
3. Double-click `AntigravityProxyLauncher.exe`.
4. Leave TUN / virtual network adapter mode disabled.

The launcher uses the default `7897` port if it cannot find a Clash configuration file.

## Build

Requirements: Windows PowerShell 5+ and .NET Framework 4.x compiler support (the build uses PowerShell `Add-Type`).

```powershell
powershell -ExecutionPolicy Bypass -File .\build_antigravity_launcher.ps1
```

Keep this layout next to the executable:

```text
AntigravityProxyLauncher.exe
payload\version.dll
payload\config.json
```

## Logs and backups

Logs and timestamped backups are stored under `%LOCALAPPDATA%\AntigravityProxyLauncher` (with a temp-directory fallback when the local app-data directory is unavailable).

## Safety and scope

This project is a Windows DLL proxy loader for Antigravity. It is intended for software you own or are authorized to modify. It does not implement a system-wide VPN or TUN adapter.

The payload in `payload/` is built from [yuaotian/antigravity-proxy v2.4](https://github.com/yuaotian/antigravity-proxy/releases/tag/v2.4). See that project for its license and source.
