using HarmonyLib;
using SickoMenu.Utils;

namespace SickoMenu.Patches;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class ChatPatches
{
    [HarmonyPrefix]
    public static bool AddChat(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer,
        [HarmonyArgument(1)] ref string chatText)
    {
        if (State.PanicMode) return true;

        if (chatText.StartsWith("/sc "))
        {
            if (State.MenuVisible)
            {
                var message = chatText[4..];
                __instance.AddChat(PlayerControl.LocalPlayer, $"[SickoMenu] {message}");
            }
            return false;
        }

        if (chatText.StartsWith("/"))
        {
            HandleCommand(chatText);
            return false;
        }

        return true;
    }

    private static void HandleCommand(string cmd)
    {
        var parts = cmd.Split(' ', 2);
        var command = parts[0].ToLower();
        var args = parts.Length > 1 ? parts[1] : "";

        switch (command)
        {
            case "/help":
                SendSickoChat("Commands: /sc [msg], /reveal, /noclip, /zoom [1-5], /wallhack, /repair");
                break;
            case "/reveal":
                State.RevealImpostors = !State.RevealImpostors;
                SendSickoChat($"Reveal Impostors: {(State.RevealImpostors ? "ON" : "OFF")}");
                break;
            case "/noclip":
                State.NoClip = !State.NoClip;
                SendSickoChat($"NoClip: {(State.NoClip ? "ON" : "OFF")}");
                break;
            case "/zoom":
                if (float.TryParse(args, out var z))
                {
                    State.Zoom = Math.Clamp(z, 0.1f, 10f);
                    SendSickoChat($"Zoom set to {State.Zoom}");
                }
                break;
            case "/wallhack":
                State.Wallhack = !State.Wallhack;
                SendSickoChat($"Wallhack: {(State.Wallhack ? "ON" : "OFF")}");
                break;
            case "/repair":
                Features.SabotageHelper.RepairAll();
                SendSickoChat("Sabotages repaired!");
                break;
            default:
                SendSickoChat($"Unknown command: {command}. Type /help");
                break;
        }
    }

    private static void SendSickoChat(string message)
    {
        var controller = HudManager.Instance?.Chat;
        if (controller != null)
        {
            controller.AddChat(PlayerControl.LocalPlayer, message);
        }
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SetVisible))]
public static class ChatVisiblePatches
{
    [HarmonyPostfix]
    public static void SetVisible(ChatController __instance, bool visible)
    {
        if (State.AlwaysShowChat && !visible && !State.PanicMode)
        {
            __instance.SetVisible(true);
        }
    }
}

[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
public static class ChatBubblePatches
{
    [HarmonyPostfix]
    public static void SetName(ChatBubble __instance, string playerName, bool isDead, bool voted, UnityEngine.Color color)
    {
        if (State.PanicMode) return;
        if (State.RevealImpostors && __instance != null)
        {
            __instance.NameText.color = UnityEngine.Color.red;
        }
    }
}
