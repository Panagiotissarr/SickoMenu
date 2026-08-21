# SickoMenu Agent Guide

## What This Is

BepInEx IL2CPP mod for Among Us 2026.8.18. C# (.NET 6), built with Harmony patches + Unity IMGUI. The repo also contains two reference codebases (`MalumMenu/`, `Hydra/`) — these are **read-only references**, not part of the build.

## Build

No local .NET SDK is installed. All builds run via GitHub Actions CI (`dotnet build` on `windows-2022`). Check CI output for compile errors.

IMPORTANT: Always push to GitHub to build the mod. The user will test it on their client

```bash
# Local (requires .NET 6 SDK + Among Us with BepInEx installed):
dotnet restore
dotnet build -c Release "-p:AmongUsDir=C:\path\to\Among Us"

# CI just runs: dotnet build -c Release
```

NuGet sources (in `nuget.config`):
- `nuget.org` — standard packages
- `nuget.bepinex.dev` — BepInEx IL2CPP libs
- `nuget.samboy.dev` — IL2CPP source stubs

Key packages (in `Directory.Build.props`):
- `BepInEx.Unity.IL2CPP` 6.0.0-be.735
- `AmongUs.GameLibs.Steam` 2026.8.18
- `BepInEx.IL2CPP.MSBuild` 2.1.0-rc.1

## Critical: Harmony Patch Disambiguation

**This is the #1 source of runtime crashes.** When a method has multiple overloads, `[HarmonyPatch(typeof(X), nameof(X.Method))]` throws `AmbiguousMatchException` and the **entire plugin fails to load**.

Methods that **require** `argumentTypes`:
- `ShipStatus.UpdateSystem` → `new Type[] { typeof(SystemTypes), typeof(PlayerControl), typeof(byte) }`
- `ShipStatus.RpcUpdateSystem` → `new Type[] { typeof(SystemTypes), typeof(byte) }`
- `Camera.ScreenToWorldPoint` → `new Type[] { typeof(UnityEngine.Vector3) }`
- `KillOverlay.ShowKillAnimation` → `new Type[] { typeof(NetworkedPlayerInfo), typeof(NetworkedPlayerInfo) }` (3 overloads exist; the other two take `OverlayKillAnimation`)

Also note: `MeetingHud.CastVote` takes `(InnerNet.PlayerId, InnerNet.PlayerId)` in 2026.8.18 — **not** bytes. Don't declare mismatched parameter types in patch methods; omit params entirely if unused.

If you add a patch targeting a method that might have overloads, always specify `argumentTypes` in the `HarmonyPatch` attribute. Check MalumMenu's patches to see if they target the same method (MalumMenu doesn't need disambiguation for methods it patches, which confirms they're unambiguous).

## Architecture

```
Plugin.cs              — Entry point: Load() registers components, applies patches, creates GUI
Patches/               — Harmony patches (one file per game class/feature area)
  ShipStatusPatches.cs — RpcUpdateSystem, SabotageSubs
  PlayerControlPatches.cs — MurderPlayer, CanMove, CompleteTask, RpcSendChat
  EOSPatches.cs        — All EOS/account/shapeshift/meeting/voting patches
  MiscPatches.cs       — Camera, Keyboard, VentCanUse/Enter/Exit, LobbyStart/Update, Physics
  ChatPatches.cs       — AddChat, ChatController.Update, SendChat, SendFreeChat
  ChatBubblePatches.cs — ChatBubble.pooledInstantiate
  ChatVisiblePatches.cs — ChatController.SetVisible
  HudManagerPatches.cs — HudManager.Update
  InnerNetClientPatches.cs — InnerNetClient.Update, OnGameJoined, KeyBinds.Update()
  MeetingHudPatches.cs — MeetingHud.Update, Awake
Gui/SickoMenuGui.cs    — Unity IMGUI menu (882 lines). MonoBehaviour with OnGUI/DrawMainMenu/DrawMenuWindow
Features/              — ESP, Replay, SabotageHelper, EventHandler
Utils/                 — GameHelper, State, KeyBinds, IL2CPP stub, Logger, PluginInfo
Offsets/               — OffsetSystem (stub)
Events/                — GameEventBus
RPC/                   — RPC handlers (MurderPlayer, CloseDoors, PlayAnimation)
SickoMenu_Native/      — Native code (separate)
```

## Key Gotchas

- **IL2CPP interop**: Types like `System.String[]`, `System.Action`, `System.Nullable<Color>` in method parameters generate warnings but work fine in private C# methods. Only matters for methods exported through IL2CPP interop.
- **No local build verification**: Always push and check GitHub Actions CI for build results.
- **Git repo is inside `Sicko Menu/`** subdirectory, not at workspace root. Run git commands with `workdir` set to `Sicko Menu/`.
- **`GameStates.Starting` does not exist** in Among Us 2026.8.18. Use `GameStates.Started` and `GameStates.Joined` only.
- **IL2CPP.cs is a no-op stub**: `EnsureInitialized()` does nothing. This file provides IL2CPP interop types for compilation without a real IL2CPP runtime.
- **Harmony patches are applied in order** in `Plugin.cs:ApplyPatches()`. If any single patch fails (ambiguous match, missing method), the entire plugin load crashes — all subsequent patches never run.

## Reference Codebases

- `MalumMenu/` — Working IL2CPP mod for the same game version. Use as reference for correct method signatures and parameter types. Do not modify.
- `Hydra/` — Another IL2CPP mod reference. Do not modify.
