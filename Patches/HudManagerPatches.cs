using HarmonyLib;
using SickoMenu.Utils;

namespace SickoMenu.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManagerPatches
{
    [HarmonyPostfix]
    public static void Update(HudManager __instance)
    {
        if (State.PanicMode) return;

        if (State.MenuVisible || State.ConsoleVisible)
            __instance.SetHudActive(false);
        else
            __instance.SetHudActive(true);

        if (State.AlwaysShowChat)
        {
            if (__instance.Chat != null)
                __instance.Chat.SetVisible(true);
        }

        if (State.FreeChat)
        {
            var freeChatField = __instance.Chat?.freeChatField;
            if (freeChatField != null)
            {
                if (freeChatField.textArea != null)
                {
                    freeChatField.textArea.AllowEngage = true;
                }
            }
        }

        if (State.BypassBans && BanMenu.Instance != null)
        {
            BanMenu.Instance.gameObject.SetActive(false);
        }
    }
}
