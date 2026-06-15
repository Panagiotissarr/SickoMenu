namespace SickoMenu.Utils;

public static class State
{
    public static bool MenuVisible { get; set; } = true;
    public static bool RadarVisible { get; set; }
    public static bool EspVisible { get; set; }
    public static bool ReplayVisible { get; set; }
    public static bool ConsoleVisible { get; set; }
    public static bool PanicMode { get; set; }
    public static bool NoClip { get; set; }
    public static bool InMeeting { get; set; }
    public static bool InGame { get; set; }
    public static bool InLobby { get; set; }

    public static bool ShowEsp { get; set; } = true;
    public static bool ShowRadar { get; set; }
    public static bool ShowReplay { get; set; }
    public static bool HideEspDuringMeetings { get; set; } = true;
    public static bool HideRadarDuringMeetings { get; set; } = true;

    public static float Zoom { get; set; } = 1.0f;
    public static bool DisableKillAnimation { get; set; }
    public static bool RevealImpostors { get; set; }
    public static bool RevealRoles { get; set; }
    public static bool Wallhack { get; set; }
    public static bool GhostMode { get; set; }
    public static bool FreeChat { get; set; } = true;
    public static bool BypassBans { get; set; }
    public static bool NoVoteCooldown { get; set; }
    public static bool AlwaysShowChat { get; set; }

    public static int SelectedTab { get; set; }
    public static int ActiveGameTab { get; set; }
    public static int ActiveHostTab { get; set; }
    public static int ActiveSelfTab { get; set; }
    public static int ActivePlayersTab { get; set; }

    public static string Username { get; set; } = "SickoUser";
    public static int PinCode { get; set; } = 0;
    public static bool EnableAuth { get; set; }
}

public static class KeyBinds
{
    public static UnityEngine.KeyCode MenuToggle { get; set; } = UnityEngine.KeyCode.Delete;
    public static UnityEngine.KeyCode RadarToggle { get; set; } = UnityEngine.KeyCode.Insert;
    public static UnityEngine.KeyCode ConsoleToggle { get; set; } = UnityEngine.KeyCode.Home;
    public static UnityEngine.KeyCode ReplayToggle { get; set; } = UnityEngine.KeyCode.End;
    public static UnityEngine.KeyCode RepairSabotage { get; set; } = UnityEngine.KeyCode.PageDown;
    public static UnityEngine.KeyCode NoClipModifier { get; set; } = UnityEngine.KeyCode.LeftControl;
    public static UnityEngine.KeyCode Panic { get; set; } = UnityEngine.KeyCode.Pause;

    private static readonly Dictionary<UnityEngine.KeyCode, bool> KeyStates = [];

    public static bool GetKeyDown(UnityEngine.KeyCode key)
    {
        var current = UnityEngine.Input.GetKey(key);
        var previous = KeyStates.GetValueOrDefault(key);
        KeyStates[key] = current;
        return current && !previous;
    }

    public static void Update()
    {
        if (GetKeyDown(Panic))
            State.PanicMode = !State.PanicMode;

        if (State.PanicMode) return;

        if (GetKeyDown(MenuToggle))
            State.MenuVisible = !State.MenuVisible;

        if (GetKeyDown(RadarToggle))
        {
            State.RadarVisible = !State.RadarVisible;
            State.ShowRadar = State.RadarVisible;
        }

        if (GetKeyDown(ConsoleToggle))
            State.ConsoleVisible = !State.ConsoleVisible;

        if (GetKeyDown(ReplayToggle))
        {
            State.ReplayVisible = !State.ReplayVisible;
            State.ShowReplay = State.ReplayVisible;
        }

        if (GetKeyDown(RepairSabotage))
            Features.SabotageHelper.RepairAll();

        State.NoClip = UnityEngine.Input.GetKey(NoClipModifier);
    }
}
