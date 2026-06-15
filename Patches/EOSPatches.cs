using HarmonyLib;
using SickoMenu.Utils;

namespace SickoMenu.Patches;

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.StartInitialLoginFlow))]
public static class EOSLoginPatches
{
    [HarmonyPrefix]
    public static bool StartInitialLoginFlow(EOSManager __instance)
    {
        if (State.PanicMode) return true;
        SickoMenuPlugin.PluginLogger.LogInfo("Bypassing EOS login flow");
        return !State.BypassBans;
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.LoginFromAccountTab))]
public static class EOSLoginTabPatches
{
    [HarmonyPrefix]
    public static bool LoginFromAccountTab()
    {
        if (State.PanicMode) return true;
        return !State.BypassBans;
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.InitializePlatformInterface))]
public static class EOSInitializePatches
{
    [HarmonyPrefix]
    public static bool InitializePlatformInterface()
    {
        if (State.PanicMode) return true;
        return !State.BypassBans;
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsFreechatAllowed))]
public static class EOSFreeChatPatches
{
    [HarmonyPostfix]
    public static void IsFreechatAllowed(ref bool __result)
    {
        if (State.PanicMode) return;
        if (State.FreeChat)
            __result = true;
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsFriendsListAllowed))]
public static class EOSFriendsPatches
{
    [HarmonyPostfix]
    public static void IsFriendsListAllowed(ref bool __result)
    {
        if (State.PanicMode) return;
        if (State.FreeChat)
            __result = true;
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.Update))]
public static class EOSUpdatePatches
{
    [HarmonyPostfix]
    public static void Update()
    {
        if (State.PanicMode) return;
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.get_ProductUserId))]
public static class EOSUserIdPatches
{
    [HarmonyPrefix]
    public static bool get_ProductUserId(ref string __result)
    {
        if (State.PanicMode) return true;
        if (State.BypassBans)
        {
            __result = "SickoMenu_SpoofedID";
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.UpdatePermissionKeys))]
public static class EOSPermissionPatches
{
    [HarmonyPrefix]
    public static bool UpdatePermissionKeys()
    {
        if (State.PanicMode) return true;
        if (State.BypassBans) return false;
        return true;
    }
}

[HarmonyPatch(typeof(AccountManager), nameof(AccountManager.UpdateKidAccountDisplay))]
public static class AccountPatches
{
    [HarmonyPrefix]
    public static bool UpdateKidAccountDisplay()
    {
        if (State.PanicMode) return true;
        return false;
    }

    [HarmonyPatch(typeof(AccountManager), nameof(AccountManager.CanPlayOnline))]
    [HarmonyPostfix]
    public static void CanPlayOnline(ref bool __result)
    {
        if (State.PanicMode) return;
        if (State.BypassBans)
            __result = true;
    }
}

[HarmonyPatch(typeof(Debug))]
internal static class DebugPatches
{
    [HarmonyPatch(nameof(Debug.Log))]
    [HarmonyPrefix]
    public static bool Log(object message)
    {
        if (State.PanicMode) return true;
        return true;
    }

    [HarmonyPatch(nameof(Debug.LogError))]
    [HarmonyPrefix]
    public static bool LogError(object message)
    {
        if (State.PanicMode) return true;
        return true;
    }

    [HarmonyPatch(nameof(Debug.LogWarning))]
    [HarmonyPrefix]
    public static bool LogWarning(object message)
    {
        if (State.PanicMode) return true;
        return true;
    }

    [HarmonyPatch(nameof(Debug.LogException))]
    [HarmonyPrefix]
    public static bool LogException(Exception exception)
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
public static class VersionShowerPatches
{
    [HarmonyPostfix]
    public static void Start(VersionShower __instance)
    {
        if (State.PanicMode) return;
        __instance.text.text += $"\nSickoMenu v{PluginInfo.PLUGIN_VERSION}";
    }
}

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.IsCharAllowed))]
public static class TextBoxPatches
{
    [HarmonyPostfix]
    public static void IsCharAllowed(ref bool __result)
    {
        if (State.PanicMode) return;
        if (State.FreeChat)
            __result = true;
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendFreeChat))]
public static class SendFreeChatPatches
{
    [HarmonyPrefix]
    public static bool SendFreeChat()
    {
        if (State.PanicMode) return true;
        if (State.FreeChat) return true;
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckVanish))]
public static class VanishPatches
{
    [HarmonyPrefix]
    public static bool CmdCheckVanish()
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckAppear))]
public static class AppearPatches
{
    [HarmonyPrefix]
    public static bool CmdCheckAppear()
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetRoleInvisibility))]
public static class InvisibilityPatches
{
    [HarmonyPrefix]
    public static bool SetRoleInvisibility()
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckProtect))]
public static class ProtectPatches
{
    [HarmonyPrefix]
    public static bool CmdCheckProtect()
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckMurder))]
public static class CmdCheckMurderPatches
{
    [HarmonyPrefix]
    public static bool CmdCheckMurder(PlayerControl __instance, PlayerControl target)
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
public static class CheckMurderPatches
{
    [HarmonyPrefix]
    public static bool CheckMurder(PlayerControl __instance, PlayerControl target)
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class HandleRpcPatches
{
    [HarmonyPrefix]
    public static bool HandleRpc(PlayerControl __instance, byte callId, MessageReader reader)
    {
        if (State.PanicMode) return true;
        SickoMenuPlugin.PluginLogger.LogInfo($"RPC: {callId}");
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class ShapeshiftPatches
{
    [HarmonyPrefix]
    public static bool Shapeshift()
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckShapeshift))]
public static class CmdCheckShapeshiftPatches
{
    [HarmonyPrefix]
    public static bool CmdCheckShapeshift()
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ProtectPlayer))]
public static class ProtectPlayerPatches
{
    [HarmonyPrefix]
    public static bool ProtectPlayer()
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcStartMeeting))]
public static class RpcStartMeetingPatches
{
    [HarmonyPrefix]
    public static void RpcStartMeeting()
    {
        State.InMeeting = true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
public static class CmdReportDeadBodyPatches
{
    [HarmonyPrefix]
    public static void CmdReportDeadBody()
    {
        State.InMeeting = true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public static class RpcSyncSettingsPatches
{
    [HarmonyPrefix]
    public static bool RpcSyncSettings()
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem))]
public static class UpdateSystemPatches
{
    [HarmonyPrefix]
    public static bool UpdateSystem(ShipStatus __instance, SystemTypes systemType, PlayerControl player, byte amount)
    {
        if (State.PanicMode) return true;
        SickoMenuPlugin.PluginLogger.LogInfo($"UpdateSystem: {systemType} by {amount}");
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoSetRole))]
public static class CoSetRolePatches
{
    [HarmonyPostfix]
    public static void CoSetRole(PlayerControl __instance, RoleTypes role, bool canOverride)
    {
        if (State.PanicMode) return;
        if (__instance.AmOwner)
            SickoMenuPlugin.PluginLogger.LogInfo($"My role: {role}");
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetLevel))]
public static class SetLevelPatches
{
    [HarmonyPrefix]
    public static bool SetLevel()
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.get_Visible))]
public static class VisiblePatches
{
    [HarmonyPostfix]
    public static void get_Visible(ref bool __result)
    {
        if (State.PanicMode) return;
        if (State.GhostMode)
            __result = true;
    }
}

[HarmonyPatch(typeof(KillButton), nameof(KillButton.SetTarget))]
public static class KillButtonPatches
{
    [HarmonyPostfix]
    public static void SetTarget(KillButton __instance, PlayerControl target)
    {
        if (State.PanicMode) return;
        if (State.Wallhack && target != null)
        {
            __instance.SetTarget(target);
        }
    }
}

[HarmonyPatch(typeof(KillOverlay), nameof(KillOverlay.ShowKillAnimation))]
public static class KillOverlayPatches
{
    [HarmonyPrefix]
    public static bool ShowKillAnimation(KillOverlay __instance,
        NetworkedPlayerInfo killer, NetworkedPlayerInfo victim)
    {
        if (State.PanicMode) return true;

        if (State.DisableKillAnimation)
            return false;

        return true;
    }
}

[HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.FindClosestTarget))]
public static class FindClosestTargetPatches
{
    [HarmonyPostfix]
    public static void FindClosestTarget(ImpostorRole __instance, ref PlayerControl __result)
    {
        if (State.PanicMode) return;

        if (State.Wallhack && __result == null)
        {
            // Find closest player ignoring walls
            var localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer == null) return;

            PlayerControl closest = null;
            float closestDist = float.MaxValue;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player == localPlayer || player.Data == null ||
                    player.Data.IsDead || player.Data.Disconnected) continue;

                var dist = Vector2.Distance(localPlayer.transform.position, player.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = player;
                }
            }

            __result = closest;
        }
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
public static class CastVotePatches
{
    [HarmonyPrefix]
    public static bool CastVote(MeetingHud __instance, byte playerId, byte suspectIdx)
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.RpcVotingComplete))]
public static class RpcVotingCompletePatches
{
    [HarmonyPrefix]
    public static void RpcVotingComplete()
    {
        State.InMeeting = false;
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
public static class CheckForEndVotingPatches
{
    [HarmonyPrefix]
    public static bool CheckForEndVoting()
    {
        if (State.PanicMode) return true;
        return true;
    }
}

[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.GetPurchase))]
public static class SaveManagerPatches
{
    [HarmonyPostfix]
    public static void GetPurchase(ref bool __result)
    {
        if (State.PanicMode) return;
        if (State.BypassBans)
            __result = true;
    }
}

[HarmonyPatch(typeof(PlayerPurchasesData), nameof(PlayerPurchasesData.GetPurchase))]
public static class PlayerPurchasesPatches
{
    [HarmonyPostfix]
    public static void GetPurchase(ref bool __result)
    {
        if (State.PanicMode) return;
        if (State.BypassBans)
            __result = true;
    }
}
