using HarmonyLib;
using SickoMenu.Utils;

namespace SickoMenu.Patches;

[HarmonyPatch]
public static class PlayerControlPatches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    public static void FixedUpdate(PlayerControl __instance)
    {
        if (State.PanicMode) return;

        if (State.NoClip)
        {
            __instance.CanMove = true;
            __instance.MyPhysics.ResetMoveState();
        }

        if (State.Zoom != 1.0f)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographicSize = 3.0f / State.Zoom;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.get_CanMove))]
    [HarmonyPrefix]
    public static bool CanMove(ref bool __result)
    {
        if (State.PanicMode) return true;

        if (State.NoClip)
        {
            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    [HarmonyPrefix]
    public static bool MurderPlayer(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
    {
        if (State.PanicMode) return true;

        if (State.DisableKillAnimation)
        {
            target.Die(DeathReason.Kill);
            __instance.MyPhysics.ResetMoveState();
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
    [HarmonyPrefix]
    public static bool CompleteTask(PlayerControl __instance, uint idx)
    {
        if (State.PanicMode) return true;

        if (__instance.AmOwner)
        {
            var data = __instance.Data;
            if (data != null && data.Tasks != null)
            {
                for (int i = 0; i < data.Tasks.Count; i++)
                {
                    var task = data.Tasks[i];
                    if (task != null && !task.Complete)
                        break;
                }
            }
        }
        return true;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
    [HarmonyPrefix]
    public static bool RpcSendChat(PlayerControl __instance, ref string chatText)
    {
        if (State.PanicMode) return true;

        if (chatText.StartsWith("/sc "))
        {
            var message = chatText[4..];
            var localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer != null)
            {
                SickoChat.SendMessage($"[SickoMenu] {message}", "#FF0000");
                return false;
            }
        }
        return true;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
    [HarmonyPrefix]
    public static void StartMeeting(PlayerControl __instance, NetworkedPlayerInfo target)
    {
        State.InMeeting = true;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnGameStart))]
    [HarmonyPostfix]
    public static void OnGameStart()
    {
        State.InGame = true;
        State.InLobby = false;
        State.InMeeting = false;
    }
}

internal static class SickoChat
{
    public static void SendMessage(string message, string colorHex = "#FF0000")
    {
        var controller = HudManager.Instance?.Chat;
        if (controller == null) return;

        ColorUtility.TryParseHtmlString(colorHex, out var color);
        controller.AddChat(PlayerControl.LocalPlayer, message);
    }
}
