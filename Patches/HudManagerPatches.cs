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

        if (State.AlwaysShowChat)
        {
            if (__instance.Chat != null)
                __instance.Chat.gameObject.SetActive(true);
        }
    }
}
