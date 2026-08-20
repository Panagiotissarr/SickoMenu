using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using SickoMenu.Events;
using SickoMenu.Features;
using SickoMenu.Gui;
using SickoMenu.Offsets;
using SickoMenu.RPC;
using UnityEngine;

namespace SickoMenu;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("Among Us.exe")]
public class SickoMenuPlugin : BasePlugin
{
    public const string PLUGIN_GUID = "dev.sicko.sickomenu";
    public const string PLUGIN_NAME = "SickoMenu";
    public const string PLUGIN_VERSION = "4.5.3";

    internal static SickoMenuPlugin Instance { get; private set; } = null!;
    internal static ManualLogSource PluginLogger { get; private set; } = null!;
    internal static Harmony HarmonyInstance { get; private set; } = null!;
    internal static System.Threading.CancellationTokenSource ShutdownCts { get; private set; } = null!;

    private GameObject? _guiObject;

    public override void Load()
    {
        Instance = this;
        PluginLogger = Log;
        HarmonyInstance = new Harmony(PLUGIN_GUID);
        ShutdownCts = new System.Threading.CancellationTokenSource();

        PluginLogger.LogInfo($"SickoMenu v{PLUGIN_VERSION} loading...");

        Utils.IL2CPP.EnsureInitialized();
        OffsetSystem.Initialize();

        RegisterComponents();
        ApplyPatches();
        InitializeSystems();
        CreateGui();

        PluginLogger.LogInfo($"SickoMenu v{PLUGIN_VERSION} loaded successfully!");
    }

    public override bool Unload()
    {
        PluginLogger.LogInfo("SickoMenu unloading...");

        ShutdownCts.Cancel();
        HarmonyInstance.UnpatchSelf();

        if (_guiObject != null)
        {
            UnityEngine.Object.Destroy(_guiObject);
            _guiObject = null;
        }

        PluginLogger.LogInfo("SickoMenu unloaded.");
        return true;
    }

    private static void RegisterComponents()
    {
        ClassInjector.RegisterTypeInIl2Cpp<SickoMenuGui>();
    }

    private void ApplyPatches()
    {
        HarmonyInstance.PatchAll(typeof(Patches.PlayerControlPatches));
        HarmonyInstance.PatchAll(typeof(Patches.MeetingHudPatches));
        HarmonyInstance.PatchAll(typeof(Patches.HudManagerPatches));
        HarmonyInstance.PatchAll(typeof(Patches.InnerNetClientPatches));
        HarmonyInstance.PatchAll(typeof(Patches.ShipStatusPatches));
        HarmonyInstance.PatchAll(typeof(Patches.ChatPatches));
        HarmonyInstance.PatchAll(typeof(Patches.ChatBubblePatches));
        HarmonyInstance.PatchAll(typeof(Patches.ChatVisiblePatches));
        HarmonyInstance.PatchAll(typeof(Patches.PhysicsPatches));
        HarmonyInstance.PatchAll(typeof(Patches.LobbyStartPatches));
        HarmonyInstance.PatchAll(typeof(Patches.LobbyUpdatePatches));
        HarmonyInstance.PatchAll(typeof(Patches.VentCanUsePatches));
        HarmonyInstance.PatchAll(typeof(Patches.VentEnterPatches));
        HarmonyInstance.PatchAll(typeof(Patches.VentExitPatches));
        HarmonyInstance.PatchAll(typeof(Patches.RolePatches));
        HarmonyInstance.PatchAll(typeof(Patches.ExilePatches));
        HarmonyInstance.PatchAll(typeof(Patches.CameraPatches));
        HarmonyInstance.PatchAll(typeof(Patches.GameStartPatches));
        HarmonyInstance.PatchAll(typeof(Patches.PingPatches));
        HarmonyInstance.PatchAll(typeof(Patches.AirshipPatches));
        HarmonyInstance.PatchAll(typeof(Patches.FunglePatches));
        HarmonyInstance.PatchAll(typeof(Patches.SabotagePatches));
        HarmonyInstance.PatchAll(typeof(Patches.KeyboardPatches));
        HarmonyInstance.PatchAll(typeof(Patches.EOSLoginPatches));
        HarmonyInstance.PatchAll(typeof(Patches.EOSLoginTabPatches));
        HarmonyInstance.PatchAll(typeof(Patches.EOSInitializePatches));
        HarmonyInstance.PatchAll(typeof(Patches.EOSFreeChatPatches));
        HarmonyInstance.PatchAll(typeof(Patches.EOSFriendsPatches));
        HarmonyInstance.PatchAll(typeof(Patches.EOSUpdatePatches));
        HarmonyInstance.PatchAll(typeof(Patches.EOSPermissionPatches));
        HarmonyInstance.PatchAll(typeof(Patches.AccountPatches));
        HarmonyInstance.PatchAll(typeof(Patches.FollowerCameraPatches));
        HarmonyInstance.PatchAll(typeof(Patches.ProtectionPatches));
        HarmonyInstance.PatchAll(typeof(Patches.GameEndPatches));
        HarmonyInstance.PatchAll(typeof(Patches.VersionShowerPatches));
        HarmonyInstance.PatchAll(typeof(Patches.TextBoxPatches));
        HarmonyInstance.PatchAll(typeof(Patches.SendFreeChatPatches));
        HarmonyInstance.PatchAll(typeof(Patches.VanishPatches));
        HarmonyInstance.PatchAll(typeof(Patches.AppearPatches));
        HarmonyInstance.PatchAll(typeof(Patches.InvisibilityPatches));
        HarmonyInstance.PatchAll(typeof(Patches.ProtectPatches));
        HarmonyInstance.PatchAll(typeof(Patches.CmdCheckMurderPatches));
        HarmonyInstance.PatchAll(typeof(Patches.CheckMurderPatches));
        HarmonyInstance.PatchAll(typeof(Patches.HandleRpcPatches));
        HarmonyInstance.PatchAll(typeof(Patches.ShapeshiftPatches));
        HarmonyInstance.PatchAll(typeof(Patches.CmdCheckShapeshiftPatches));
        HarmonyInstance.PatchAll(typeof(Patches.ProtectPlayerPatches));
        HarmonyInstance.PatchAll(typeof(Patches.RpcStartMeetingPatches));
        HarmonyInstance.PatchAll(typeof(Patches.CmdReportDeadBodyPatches));
        HarmonyInstance.PatchAll(typeof(Patches.RpcSyncSettingsPatches));
        HarmonyInstance.PatchAll(typeof(Patches.UpdateSystemPatches));
        HarmonyInstance.PatchAll(typeof(Patches.SetLevelPatches));
        HarmonyInstance.PatchAll(typeof(Patches.KillButtonPatches));
        HarmonyInstance.PatchAll(typeof(Patches.KillOverlayPatches));
        HarmonyInstance.PatchAll(typeof(Patches.FindClosestTargetPatches));
        HarmonyInstance.PatchAll(typeof(Patches.CastVotePatches));
        HarmonyInstance.PatchAll(typeof(Patches.RpcVotingCompletePatches));
        HarmonyInstance.PatchAll(typeof(Patches.CheckForEndVotingPatches));
        HarmonyInstance.PatchAll(typeof(Patches.PlayerPurchasesPatches));
    }

    private static void InitializeSystems()
    {
        GameEventBus.Clear();
        ReplaySystem.Clear();
        RpcMurderPlayerHandler.Reset();
        RpcCloseDoorsOfTypeHandler.Reset();
        RpcPlayAnimationHandler.Reset();
        EspRenderer.Clear();

        GameEventBus.OnEvent += OnGameEvent;
    }

    private void CreateGui()
    {
        _guiObject = new GameObject("SickoMenuGUI");
        _guiObject.AddComponent<SickoMenuGui>();
        UnityEngine.Object.DontDestroyOnLoad(_guiObject);
    }

    private static void OnGameEvent(object? sender, GameEvent evt)
    {
        switch (evt.Type)
        {
            case GameEventType.Kill:
            case GameEventType.Murder:
                LogEvent("KILL", evt);
                break;
            case GameEventType.MeetingStart:
                LogEvent("MEETING", evt);
                break;
            case GameEventType.Disconnect:
                LogEvent("DISCONNECT", evt);
                break;
            case GameEventType.GameStart:
                PluginLogger.LogInfo("=== GAME STARTED ===");
                break;
            case GameEventType.GameEnd:
                PluginLogger.LogInfo($"=== GAME ENDED: {evt.Data.GetValueOrDefault("EndReason")} ===");
                break;
            case GameEventType.CheatDetected:
                PluginLogger.LogWarning($"CHEAT: {evt.Data.GetValueOrDefault("CheatType")}");
                break;
        }
    }

    private static void LogEvent(string prefix, GameEvent evt)
    {
        PluginLogger.LogInfo(
            $"[{prefix}] P{evt.SourcePlayerId}" +
            (evt.TargetPlayerId.HasValue ? $" -> P{evt.TargetPlayerId}" : ""));
    }
}
