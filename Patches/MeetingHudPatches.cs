using HarmonyLib;
using SickoMenu.Utils;

namespace SickoMenu.Patches;

[HarmonyPatch]
public static class MeetingHudPatches
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    [HarmonyPostfix]
    public static void Update(MeetingHud __instance)
    {
        if (State.PanicMode) return;
        State.InMeeting = true;
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Awake))]
    [HarmonyPostfix]
    public static void Awake(MeetingHud __instance)
    {
        State.InMeeting = true;
    }

    [HarmonyPatch(typeof(MeetingHud), "Close")]
    [HarmonyPostfix]
    public static void Close()
    {
        State.InMeeting = false;
    }

    private static UnityEngine.Color GetRoleColor(RoleBehaviour role)
    {
        if (role == null) return UnityEngine.Color.white;
        return role.TeamType switch
        {
            RoleTeamTypes.Impostor => UnityEngine.Color.red,
            RoleTeamTypes.Crewmate => UnityEngine.Color.cyan,
            _ => UnityEngine.Color.white
        };
    }
}
