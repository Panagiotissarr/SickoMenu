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
    internal static SickoMenuPlugin Instance { get; private set; } = null!;
    internal static ManualLogSource PluginLogger { get; private set; } = null!;
    internal static Harmony HarmonyInstance { get; private set; } = null!;
    internal static System.Threading.CancellationTokenSource ShutdownCts { get; private set; } = null!;

    internal static SickoMenuGui? Gui { get; private set; }

    public override void Load()
    {
        Instance = this;
        PluginLogger = Log;
        HarmonyInstance = new Harmony(PluginInfo.PLUGIN_GUID);
        ShutdownCts = new System.Threading.CancellationTokenSource();

        PluginLogger.LogInfo($"SickoMenu v{PluginInfo.PLUGIN_VERSION} loading...");

        Utils.IL2CPP.EnsureInitialized();
        OffsetSystem.Initialize();

        RegisterComponents();
        ApplyPatches();
        InitializeSystems();
        CreateGui();

        PluginLogger.LogInfo($"SickoMenu v{PluginInfo.PLUGIN_VERSION} loaded successfully!");
    }

    public override bool Unload()
    {
        PluginLogger.LogInfo("SickoMenu unloading...");

        ShutdownCts.Cancel();
        HarmonyInstance.UnpatchSelf();

        if (Gui != null)
        {
            UnityEngine.Object.Destroy(Gui);
            Gui = null;
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
        HarmonyInstance.PatchAll();
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
        // Same pattern as Hydra/MalumMenu: BepInEx's AddComponent<T>() hosts the
        // component on the persistent "BepInEx_Manager" object (HideAndDontSave),
        // which survives scene changes without DontDestroyOnLoad.
        Gui = AddComponent<SickoMenuGui>();
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
