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

        if (State.RevealImpostors || State.RevealRoles)
        {
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                var playerState = __instance.playerStates[i];
                if (playerState == null) continue;

                var data = playerState.TargetPlayerData;
                if (data == null) continue;

                if (State.RevealImpostors && data.Role != null && data.Role.IsImpostor)
                {
                    playerState.NameText.color = UnityEngine.Color.red;
                    playerState.NameText.text = $"{data.PlayerName} [IMP]";
                }
                else if (State.RevealRoles && data.Role != null)
                {
                    playerState.NameText.color = GetRoleColor(data.Role);
                    playerState.NameText.text = $"{data.PlayerName} [{data.Role.RoleType}]";
                }
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
    [HarmonyPostfix]
    public static void PopulateResults(MeetingHud __instance, [HarmonyArgument(0)] Il2CppReferenceArray<MeetingHud.VoterState> states)
    {
        if (State.PanicMode) return;

        if (State.RevealImpostors)
        {
            foreach (var playerVoteArea in __instance.playerStates)
            {
                var data = playerVoteArea.TargetPlayerData;
                if (data == null || data.Role == null) continue;

                if (data.Role.IsImpostor)
                {
                    playerVoteArea.NameText.color = UnityEngine.Color.red;
                }
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Awake))]
    [HarmonyPostfix]
    public static void Awake(MeetingHud __instance)
    {
        State.InMeeting = true;
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
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
            RoleTeamTypes.Alone => UnityEngine.Color.magenta,
            _ => UnityEngine.Color.white
        };
    }
}
