using HarmonyLib;
using SickoMenu.Utils;
using UnityEngine;

namespace SickoMenu.Patches;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
public static class PhysicsPatches
{
    [HarmonyPostfix]
    public static void FixedUpdate(PlayerPhysics __instance)
    {
        if (State.PanicMode) return;

        if (State.NoClip && __instance.AmOwner)
        {
        }
    }
}

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class LobbyStartPatches
{
    [HarmonyPostfix]
    public static void Start()
    {
        State.InLobby = true;
        State.InGame = false;
        State.InMeeting = false;
    }
}

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Update))]
public static class LobbyUpdatePatches
{
    [HarmonyPostfix]
    public static void Update()
    {
        if (State.PanicMode) return;
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
public static class VentCanUsePatches
{
    [HarmonyPostfix]
    public static void CanUse(Vent __instance, NetworkedPlayerInfo pc, ref float __result, ref bool canUse, ref bool couldUse)
    {
        if (State.PanicMode) return;
        if (pc != null && pc.PlayerId == PlayerControl.LocalPlayer?.PlayerId)
        {
            canUse = true;
            couldUse = true;
            __result = 0f;
        }
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
public static class VentEnterPatches
{
    [HarmonyPostfix]
    public static void EnterVent()
    {
        SickoMenuPlugin.PluginLogger.LogInfo("Entered vent");
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
public static class VentExitPatches
{
    [HarmonyPostfix]
    public static void ExitVent()
    {
        SickoMenuPlugin.PluginLogger.LogInfo("Exited vent");
    }
}

[HarmonyPatch(typeof(RoleManager), "SelectRoles")]
public static class RolePatches
{
    [HarmonyPostfix]
    public static void SelectRoles()
    {
        if (State.PanicMode) return;
        SickoMenuPlugin.PluginLogger.LogInfo("Roles selected");
    }
}

[HarmonyPatch("ExileController", "ReEnableGameplay")]
public static class ExilePatches
{
    [HarmonyPostfix]
    public static void ReEnableGameplay()
    {
        State.InMeeting = false;
    }
}

[HarmonyPatch(typeof(Camera), nameof(Camera.ScreenToWorldPoint))]
public static class CameraPatches
{
    [HarmonyPostfix]
    public static void ScreenToWorldPoint(Camera __instance, UnityEngine.Vector3 position, ref UnityEngine.Vector3 __result)
    {
        if (State.PanicMode) return;

        if (State.Wallhack)
        {
            __result = position;
        }
    }
}

[HarmonyPatch(typeof(GameStartManager), "Update")]
public static class GameStartPatches
{
    [HarmonyPostfix]
    public static void Update(GameStartManager __instance)
    {
        if (State.PanicMode) return;
    }
}

[HarmonyPatch("PingTracker", "Update")]
public static class PingPatches
{
    [HarmonyPostfix]
    public static void Update(PingTracker __instance)
    {
        if (State.PanicMode) return;
    }
}

[HarmonyPatch("KeyboardJoystick", "Update")]
public static class KeyboardPatches
{
    [HarmonyPrefix]
    public static bool Update(KeyboardJoystick __instance)
    {
        if (State.PanicMode) return true;

        if (State.NoClip)
        {
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(FollowerCamera), "Update")]
public static class FollowerCameraPatches
{
    [HarmonyPrefix]
    public static bool Update(FollowerCamera __instance)
    {
        if (State.PanicMode) return true;

        if (State.Zoom != 1.0f)
        {
            Camera main = Camera.main;
            if (main != null)
            {
                main.orthographicSize = 3.0f / State.Zoom;
            }
        }
        return true;
    }
}

[HarmonyPatch("PlayerControl", "TurnOnProtection")]
public static class ProtectionPatches
{
    [HarmonyPrefix]
    public static bool TurnOnProtection()
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(AmongUsClient), "OnGameEnd")]
public static class GameEndPatches
{
    [HarmonyPostfix]
    public static void OnGameEnd()
    {
        State.InGame = false;
        State.InMeeting = false;
        State.InLobby = false;
    }
}


