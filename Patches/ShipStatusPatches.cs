using HarmonyLib;
using SickoMenu.Utils;

namespace SickoMenu.Patches;

[HarmonyPatch("ShipStatus", "OnEnable")]
public static class ShipStatusPatches
{
    [HarmonyPostfix]
    public static void OnEnable(ShipStatus __instance)
    {
        State.InGame = true;
        State.InLobby = false;
        SickoMenuPlugin.PluginLogger.LogInfo($"ShipStatus OnEnable: {__instance?.Type}");
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    [HarmonyPostfix]
    public static void CalculateLightRadius(ShipStatus __instance, NetworkedPlayerInfo player, ref float __result)
    {
        if (State.PanicMode) return;
        if (State.Wallhack)
            __result = 100f;
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcUpdateSystem))]
    [HarmonyPrefix]
    public static bool RpcUpdateSystem(ShipStatus __instance, SystemTypes systemType, int amount)
    {
        if (State.PanicMode) return true;
        SickoMenuPlugin.PluginLogger.LogInfo($"RpcUpdateSystem: {systemType} = {amount}");
        return true;
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcCloseDoorsOfType))]
    [HarmonyPrefix]
    public static bool RpcCloseDoorsOfType(ShipStatus __instance, SystemTypes type)
    {
        if (State.PanicMode) return true;
        SickoMenuPlugin.PluginLogger.LogInfo($"RpcCloseDoorsOfType: {type}");
        return true;
    }
}

[HarmonyPatch("AirshipStatus", "OnEnable")]
public static class AirshipPatches
{
    [HarmonyPostfix]
    public static void OnEnable(AirshipStatus __instance)
    {
        State.InGame = true;
    }

    [HarmonyPatch(typeof(AirshipStatus), nameof(AirshipStatus.CalculateLightRadius))]
    [HarmonyPostfix]
    public static void CalculateLightRadius(NetworkedPlayerInfo player, ref float __result)
    {
        if (State.PanicMode) return;
        if (State.Wallhack)
            __result = 100f;
    }
}

[HarmonyPatch("FungleShipStatus", "OnEnable")]
public static class FunglePatches
{
    [HarmonyPostfix]
    public static void OnEnable(FungleShipStatus __instance)
    {
        State.InGame = true;
    }
}

[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.SetInitialSabotageCooldown))]
public static class SabotagePatches
{
    [HarmonyPrefix]
    public static bool SetInitialSabotageCooldown(SabotageSystemType __instance)
    {
        if (State.PanicMode) return true;
        return false;
    }
}
