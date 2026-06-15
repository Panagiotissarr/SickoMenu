using HarmonyLib;
using SickoMenu.Utils;

namespace SickoMenu.Patches;

[HarmonyPatch]
public static class InnerNetClientPatches
{
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Update))]
    [HarmonyPostfix]
    public static void Update(InnerNetClient __instance)
    {
        if (State.PanicMode) return;

        KeyBinds.Update();

        State.InGame = GameHelper.IsInGame();
        State.InLobby = GameHelper.IsInLobby();

        if (State.InGame)
            State.InMeeting = GameHelper.IsInMeeting();

        if (__instance.GameState == InnerNetClient.GameStates.Started)
        {
            State.InGame = true;
            State.InLobby = false;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    [HarmonyPostfix]
    public static void OnGameJoined(AmongUsClient __instance, [HarmonyArgument(0)] string gameIdString)
    {
        SickoMenuPlugin.PluginLogger.LogInfo($"Joined game: {gameIdString}");
        State.InGame = true;
        State.InLobby = true;
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    [HarmonyPostfix]
    public static void OnPlayerLeft(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data,
        [HarmonyArgument(1)] DisconnectReasons reason)
    {
        SickoMenuPlugin.PluginLogger.LogInfo($"Player left: {data?.PlayerName}");
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    [HarmonyPostfix]
    public static void OnPlayerJoined(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data)
    {
        SickoMenuPlugin.PluginLogger.LogInfo($"Player joined: {data?.PlayerName}");
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    [HarmonyPostfix]
    public static void OnGameEnd()
    {
        State.InGame = false;
        State.InMeeting = false;
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.EnqueueDisconnect))]
    [HarmonyPrefix]
    public static bool EnqueueDisconnect(InnerNetClient __instance,
        [HarmonyArgument(0)] DisconnectReasons reason,
        [HarmonyArgument(1)] string stringReason)
    {
        if (State.PanicMode) return true;

        if (reason == DisconnectReasons.Banned)
        {
            SickoMenuPlugin.PluginLogger.LogWarning($"Blocked ban disconnect: {stringReason}");
            if (State.BypassBans)
                return false;
        }
        return true;
    }
}
