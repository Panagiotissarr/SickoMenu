# SickoMenu BepInEx

A BepInEx IL2CPP mod for Among Us, ported from the original SickoMenu C++ internal cheat.

## Building

### Prerequisites
- .NET 6 SDK
- Among Us with BepInEx 6 IL2CPP installed

### Local Build
```bash
set AmongUsDir="C:\path\to\Among Us"
dotnet build
```

### CI Build
The included `.github/workflows/build.yml` builds via GitHub Actions on push/PR/release.

## Installation
1. Install BepInEx 6 IL2CPP for Among Us
2. Copy `SickoMenu.dll` to `BepInEx/plugins/SickoMenu/`
3. Launch Among Us

## Default Hotkeys
- DELETE - Toggle Menu
- INSERT - Toggle Radar
- HOME - Toggle Console
- END - Toggle Replay
- PAGE DOWN - Repair Sabotage
- CTRL (hold) - NoClip
- PAUSE/BREAK - Panic (disable all features)

## License
GPL-3.0
