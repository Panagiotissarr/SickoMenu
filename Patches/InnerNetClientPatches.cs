using HarmonyLib;
using InnerNet;
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

    [HarmonyPatch("AmongUsClient", "OnGameJoined")]
    [HarmonyPostfix]
    public static void OnGameJoined(object __instance, [HarmonyArgument(0)] string gameIdString)
    {
        SickoMenuPlugin.PluginLogger.LogInfo($"Joined game: {gameIdString}");
        State.InGame = true;
        State.InLobby = true;
    }

    [HarmonyPatch("AmongUsClient", "OnPlayerLeft")]
    [HarmonyPostfix]
    public static void OnPlayerLeft(object __instance, [HarmonyArgument(0)] object data,
        [HarmonyArgument(1)] object reason)
    {
        SickoMenuPlugin.PluginLogger.LogInfo($"Player left: {data}");
    }

    [HarmonyPatch("AmongUsClient", "OnPlayerJoined")]
    [HarmonyPostfix]
    public static void OnPlayerJoined(object __instance, [HarmonyArgument(0)] object data)
    {
        SickoMenuPlugin.PluginLogger.LogInfo($"Player joined: {data}");
    }

    [HarmonyPatch("AmongUsClient", "OnGameEnd")]
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
