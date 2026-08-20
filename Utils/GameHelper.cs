using UnityEngine;
using InnerNet;

namespace SickoMenu.Utils;

public static class GameHelper
{
    public static bool IsInGame()
    {
        try
        {
            if (AmongUsClient.Instance == null) return false;
            return AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started ||
                   AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Starting;
        }
        catch { return false; }
    }

    public static bool IsInLobby()
    {
        try
        {
            if (AmongUsClient.Instance == null) return false;
            return AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined;
        }
        catch { return false; }
    }

    public static bool IsInMeeting()
    {
        try
        {
            return MeetingHud.Instance != null;
        }
        catch { return false; }
    }

    public static bool IsHost()
    {
        try
        {
            return AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost;
        }
        catch { return false; }
    }

    public static int GetPing()
    {
        try
        {
            if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmClient)
                return AmongUsClient.Instance.Ping;
            return 0;
        }
        catch { return 0; }
    }
}
