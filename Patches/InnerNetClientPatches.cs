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

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    [HarmonyPostfix]
    public static void OnGameJoined(string gameIdString)
    {
        SickoMenuPlugin.PluginLogger.LogInfo($"Joined game: {gameIdString}");
        State.InGame = true;
        State.InLobby = true;
    }

    [HarmonyPatch(typeof(AmongUsClient), "OnPlayerLeft")]
    [HarmonyPostfix]
    public static void OnPlayerLeft(object data, object reason)
    {
        SickoMenuPlugin.PluginLogger.LogInfo("Player left");
    }

    [HarmonyPatch(typeof(AmongUsClient), "OnPlayerJoined")]
    [HarmonyPostfix]
    public static void OnPlayerJoined(object data)
    {
        SickoMenuPlugin.PluginLogger.LogInfo("Player joined");
    }
}
