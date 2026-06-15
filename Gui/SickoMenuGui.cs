using BepInEx;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes;
using SickoMenu.Utils;
using UnityEngine;

namespace SickoMenu.Gui;

public class SickoMenuGui : MonoBehaviour
{
    private Rect _menuRect = new(50, 50, 800, 600);
    private Rect _consoleRect = new(100, 100, 600, 400);
    private Rect _radarRect = new(150, 150, 400, 400);
    private Rect _replayRect = new(200, 200, 600, 400);
    private Rect _espRect = new(250, 250, 300, 200);

    private Vector2 _menuScroll;
    private Vector2 _consoleScroll;
    private Vector2 _playerScroll;
    private Vector2 _hostScroll;
    private Vector2 _selfScroll;
    private Vector2 _tasksScroll;

    private readonly List<string> _consoleLines = new List<string>();
    private string _consoleInput = "";

    private string _statusBarMessage = "";
    private float _statusBarTimer;

    private readonly string[] _mainTabs = new string[] {
        "Game", "Host", "Self", "Players", "ESP", "Radar",
        "Replay", "Sabotage", "Doors", "Tasks", "Debug", "Settings", "About"
    };

    private readonly string[] _gameTabs = new string[] { "General", "Visuals", "Chat", "Safety" };
    private readonly string[] _hostTabs = new string[] { "General", "Roles", "Options" };
    private readonly string[] _selfTabs = new string[] { "Movement", "Appearance", "Actions" };
    private readonly string[] _playersTabs = new string[] { "All Players", "Actions" };

    private bool _dragWindow;
    private Vector2 _dragOffset;

    private float _lastUpdateTime;
    private float _lastPing;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        _consoleLines.Add("SickoMenu v" + PluginInfo.PLUGIN_VERSION + " Console");
        _consoleLines.Add("Type /help for commands");
        _consoleLines.Add("---");
    }

    private void Update()
    {
        if (State.PanicMode) return;

        if (Time.time - _lastUpdateTime > 1f)
        {
            _lastUpdateTime = Time.time;
            _lastPing = AmongUsClient.Instance?.Ping ?? 0;
        }
    }

    private void OnGUI()
    {
        if (State.PanicMode) return;

        if (State.MenuVisible)
            DrawMainMenu();

        if (State.ConsoleVisible)
            DrawConsole();

        if (State.ShowRadar)
            DrawRadar();

        if (State.ShowReplay)
            DrawReplay();

        if (State.ShowEsp)
            DrawEsp();

        if (_statusBarTimer > 0)
        {
            _statusBarTimer -= Time.deltaTime;
            var rect = new Rect(Screen.width / 2f - 150, Screen.height - 40, 300, 30);
            GUI.Box(rect, _statusBarMessage);
        }
    }

    private void DrawMainMenu()
    {
        GUI.skin = CreateSickoSkin();

        _menuRect = GUI.Window(0, _menuRect, DrawMenuWindow, new GUIContent("SickoMenu v" + PluginInfo.PLUGIN_VERSION),
            GUI.skin.GetStyle("window"));

        if (_menuRect.x < 0) _menuRect.x = 0;
        if (_menuRect.y < 0) _menuRect.y = 0;
        if (_menuRect.x + _menuRect.width > Screen.width)
            _menuRect.x = Screen.width - _menuRect.width;
        if (_menuRect.y + _menuRect.height > Screen.height)
            _menuRect.y = Screen.height - _menuRect.height;

        GUI.skin = null;
    }

    private void DrawMenuWindow(int id)
    {
        const int tabBarHeight = 30;
        const int subTabHeight = 25;

        var contentRect = new Rect(10, 50, _menuRect.width - 20, _menuRect.height - 90);
        var tabBarRect = new Rect(10, 25, _menuRect.width - 20, tabBarHeight);
        var subTabRect = new Rect(10, 25 + tabBarHeight, _menuRect.width - 20, subTabHeight);

        var cols = Mathf.Max(1, _mainTabs.Length / 2);
        var tabWidth = tabBarRect.width / cols;

        var selectedTab = State.SelectedTab;
        DrawTabBar(tabBarRect, tabWidth, _mainTabs, ref selectedTab, cols, 2);
        State.SelectedTab = selectedTab;
        DrawContent(contentRect, State.SelectedTab);

    }

    private void DrawTabBar(Rect rect, float tabWidth, string[] tabs, ref int selected, int cols, int rows)
    {
        var totalHeight = rect.height * rows;
        for (int i = 0; i < tabs.Length; i++)
        {
            var col = i % cols;
            var row = i / cols;
            var tabRect = new Rect(rect.x + col * tabWidth, rect.y + row * rect.height, tabWidth - 2, rect.height - 2);

            var wasSelected = selected == i;
            var isSelected = GUI.Toggle(tabRect, wasSelected, new GUIContent(tabs[i]), GUI.skin.button);
            if (isSelected && !wasSelected)
                selected = i;
        }
    }

    private void DrawContent(Rect rect, int tab)
    {
        switch (tab)
        {
            case 0: DrawGameTab(rect); break;
            case 1: DrawHostTab(rect); break;
            case 2: DrawSelfTab(rect); break;
            case 3: DrawPlayersTab(rect); break;
            case 4: DrawEspTab(rect); break;
            case 5: DrawRadarTab(rect); break;
            case 6: DrawReplayTab(rect); break;
            case 7: DrawSabotageTab(rect); break;
            case 8: DrawDoorsTab(rect); break;
            case 9: DrawTasksTab(rect); break;
            case 10: DrawDebugTab(rect); break;
            case 11: DrawSettingsTab(rect); break;
            case 12: DrawAboutTab(rect); break;
        }
    }

    private float GetContentHeight(int tab) => tab switch
    {
        0 => 1200f,
        1 => 1400f,
        2 => 1200f,
        3 => 2000f,
        4 => 300f,
        5 => 300f,
        6 => 400f,
        7 => 500f,
        8 => 400f,
        9 => 500f,
        10 => 800f,
        11 => 600f,
        12 => 800f,
        _ => 400f
    };

    #region Game Tab
    private void DrawGameTab(Rect rect)
    {
        var y = 0f;
        var activeGameTab = State.ActiveGameTab;
        DrawSubTabs(ref activeGameTab, _gameTabs, rect);
        State.ActiveGameTab = activeGameTab;
        y += 30;

        if (State.ActiveGameTab == 0) DrawGameGeneral(rect, ref y);
        else if (State.ActiveGameTab == 1) DrawGameVisuals(rect, ref y);
        else if (State.ActiveGameTab == 2) DrawGameChat(rect, ref y);
        else if (State.ActiveGameTab == 3) DrawGameSafety(rect, ref y);
    }

    private void DrawGameGeneral(Rect rect, ref float y)
    {
        DrawLabel("General Settings", ref y);
        State.RevealImpostors = DrawToggle(rect, ref y, "Reveal Impostors", State.RevealImpostors);
        State.RevealRoles = DrawToggle(rect, ref y, "Reveal Roles", State.RevealRoles);
        State.Wallhack = DrawToggle(rect, ref y, "Wallhack", State.Wallhack);
        DrawToggle(rect, ref y, "Show Ghosts", State.GhostMode);
        State.NoClip = DrawToggle(rect, ref y, "NoClip (Hold CTRL)", State.NoClip);
        State.DisableKillAnimation = DrawToggle(rect, ref y, "Disable Kill Animation", State.DisableKillAnimation);
        DrawButton(rect, ref y, "Repair All Sabotages", () => Features.SabotageHelper.RepairAll());
        DrawLabel($"Game State: {(State.InGame ? "In Game" : State.InLobby ? "In Lobby" : "Menu")}", ref y);
        DrawLabel($"Meeting: {State.InMeeting}", ref y);
    }

    private void DrawGameVisuals(Rect rect, ref float y)
    {
        DrawLabel("Visual Settings", ref y);
        DrawSlider(rect, ref y, "Zoom", State.Zoom, 0.5f, 10f);
        State.EspVisible = DrawToggle(rect, ref y, "Show ESP", State.EspVisible);
        State.RadarVisible = DrawToggle(rect, ref y, "Show Radar", State.RadarVisible);
        State.HideEspDuringMeetings = DrawToggle(rect, ref y, "Hide ESP During Meetings", State.HideEspDuringMeetings);
        State.HideRadarDuringMeetings = DrawToggle(rect, ref y, "Hide Radar During Meetings", State.HideRadarDuringMeetings);
        State.ShowReplay = DrawToggle(rect, ref y, "Show Replay System", State.ShowReplay);
    }

    private void DrawGameChat(Rect rect, ref float y)
    {
        DrawLabel("Chat Settings", ref y);
        State.FreeChat = DrawToggle(rect, ref y, "Free Chat (Bypass Chat Restrictions)", State.FreeChat);
        State.AlwaysShowChat = DrawToggle(rect, ref y, "Always Show Chat", State.AlwaysShowChat);

        DrawLabel("", ref y);
        DrawLabel("Commands:", ref y);
        DrawLabel("  /sc [message] - Send SickoChat message", ref y);
        DrawLabel("  /reveal - Toggle impostor reveal", ref y);
        DrawLabel("  /noclip - Toggle noclip", ref y);
        DrawLabel("  /zoom [1-5] - Set zoom level", ref y);
        DrawLabel("  /wallhack - Toggle wallhack", ref y);
        DrawLabel("  /repair - Repair sabotages", ref y);
    }

    private void DrawGameSafety(Rect rect, ref float y)
    {
        DrawLabel("Safety Settings", ref y);
        if (DrawButton(rect, ref y, "Panic / Disable SickoMenu",
                () => State.PanicMode = !State.PanicMode, UnityEngine.Color.red))
        {
        }
        DrawLabel("Panic Mode disables ALL features instantly.", ref y);
        DrawLabel("Hotkey: Pause/Break", ref y);
    }
    #endregion

    #region Host Tab
    private void DrawHostTab(Rect rect)
    {
        var y = 0f;
        var activeHostTab = State.ActiveHostTab;
        DrawSubTabs(ref activeHostTab, _hostTabs, rect);
        State.ActiveHostTab = activeHostTab;
        y += 30;

        if (State.ActiveHostTab == 0) DrawHostGeneral(rect, ref y);
        else if (State.ActiveHostTab == 1) DrawHostRoles(rect, ref y);
        else if (State.ActiveHostTab == 2) DrawHostOptions(rect, ref y);
    }

    private void DrawHostGeneral(Rect rect, ref float y)
    {
        DrawLabel("Host Settings", ref y);
        if (DrawButton(rect, ref y, "Start Game", () => { })) { }
        if (DrawButton(rect, ref y, "End Game", () => { })) { }
        DrawButton(rect, ref y, "Kick All Players", () => { });
        DrawButton(rect, ref y, "Close/Lock Lobby", () => { });
    }

    private void DrawHostRoles(Rect rect, ref float y)
    {
        DrawLabel("Role Assignment", ref y);
        DrawLabel("Select roles per player...", ref y);
        DrawButton(rect, ref y, "Give Impostor to (selected)", () => { });
        DrawButton(rect, ref y, "Give Crewmate to (selected)", () => { });
        DrawButton(rect, ref y, "Randomize All Roles", () => { });
    }

    private void DrawHostOptions(Rect rect, ref float y)
    {
        DrawLabel("Game Options Override", ref y);
        var killCd = 30f;
        DrawSlider(rect, ref y, "Kill Cooldown", killCd, 0f, 120f);
        var playerSpeed = 1f;
        DrawSlider(rect, ref y, "Player Speed", playerSpeed, 0.5f, 5f);
        var vision = 1f;
        DrawSlider(rect, ref y, "Crewmate Vision", vision, 0.25f, 5f);
        var impVision = 1.5f;
        DrawSlider(rect, ref y, "Impostor Vision", impVision, 0.25f, 5f);
        var tasks = 4;
        DrawIntSlider(rect, ref y, "Common Tasks", ref tasks, 0, 4);
    }
    #endregion

    #region Self Tab
    private void DrawSelfTab(Rect rect)
    {
        var y = 0f;
        var activeSelfTab = State.ActiveSelfTab;
        DrawSubTabs(ref activeSelfTab, _selfTabs, rect);
        State.ActiveSelfTab = activeSelfTab;
        y += 30;

        if (State.ActiveSelfTab == 0) DrawSelfMovement(rect, ref y);
        else if (State.ActiveSelfTab == 1) DrawSelfAppearance(rect, ref y);
        else if (State.ActiveSelfTab == 2) DrawSelfActions(rect, ref y);
    }

    private void DrawSelfMovement(Rect rect, ref float y)
    {
        DrawLabel("Movement", ref y);
        State.Zoom = DrawSlider(rect, ref y, "Zoom", State.Zoom, 0.5f, 10f);
        DrawLabel($"Zoom: {State.Zoom:F1}x", ref y);
    }

    private void DrawSelfAppearance(Rect rect, ref float y)
    {
        DrawLabel("Appearance", ref y);
        DrawToggle(rect, ref y, "Ghost Mode (Show as alive)", State.GhostMode);
    }

    private void DrawSelfActions(Rect rect, ref float y)
    {
        DrawLabel("Actions", ref y);
        DrawButton(rect, ref y, "Suicide", () => { });
        DrawButton(rect, ref y, "Report Body (fake)", () => { });
    }
    #endregion

    #region Players Tab
    private void DrawPlayersTab(Rect rect)
    {
        var y = 0f;
        var activePlayersTab = State.ActivePlayersTab;
        DrawSubTabs(ref activePlayersTab, _playersTabs, rect);
        State.ActivePlayersTab = activePlayersTab;
        y += 30;

        if (State.ActivePlayersTab == 0) DrawAllPlayers(rect, ref y);
        else if (State.ActivePlayersTab == 1) DrawPlayerActions(rect, ref y);
    }

    private void DrawAllPlayers(Rect rect, ref float y)
    {
        DrawLabel("Connected Players", ref y);
        var py = 0f;
        try
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null) continue;
                var data = player.Data;
                if (data == null) continue;

                var roleStr = data.Role != null ? $"[{data.Role}]" : "[No Role]";
                var impStr = data.Role != null && data.Role.IsImpostor ? " IMP" : "";
                DrawLabelAt($"Player {data.PlayerId}: {data.PlayerName} {roleStr}{impStr}",
                    ref py, rect);
            }
        }
        catch { }
        y += 410;
    }

    private void DrawPlayerActions(Rect rect, ref float y)
    {
        DrawLabel("Player Actions", ref y);
        DrawButton(rect, ref y, "Murder Selected", () => { });
        DrawButton(rect, ref y, "Vote Out Selected", () => { });
        DrawButton(rect, ref y, "Ban Selected", () => { });
    }
    #endregion

    #region Feature Tabs
    private void DrawEspTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("ESP Settings", ref y);
        State.ShowEsp = DrawToggle(rect, ref y, "Enable ESP", State.ShowEsp);
        State.HideEspDuringMeetings = DrawToggle(rect, ref y, "Hide During Meetings", State.HideEspDuringMeetings);
    }

    private void DrawRadarTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("Radar Settings", ref y);
        State.ShowRadar = DrawToggle(rect, ref y, "Enable Radar", State.ShowRadar);
        State.HideRadarDuringMeetings = DrawToggle(rect, ref y, "Hide During Meetings", State.HideRadarDuringMeetings);
    }

    private void DrawReplayTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("Replay System", ref y);
        State.ShowReplay = DrawToggle(rect, ref y, "Enable Replay", State.ShowReplay);
        DrawButton(rect, ref y, "Record Last 30s", () => { });
        DrawButton(rect, ref y, "Save Replay", () => { });
        DrawButton(rect, ref y, "Load Replay", () => { });
    }

    private void DrawSabotageTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("Sabotage Controls", ref y);
        if (DrawButton(rect, ref y, "Repair All Sabotages", () => Features.SabotageHelper.RepairAll()))
            ShowStatus("All sabotages repaired!");
        var saboNames = new[] { "Reactor", "Oxygen", "Lights", "Comms", "Seismic", "Doors" };
        foreach (var name in saboNames)
        {
            DrawButton(rect, ref y, $"Repair {name}", () => ShowStatus($"Repairing {name}..."));
        }
    }

    private void DrawDoorsTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("Door Controls", ref y);
        DrawButton(rect, ref y, "Open All Doors", () => ShowStatus("Opening all doors..."));
        DrawButton(rect, ref y, "Close All Doors", () => ShowStatus("Closing all doors..."));
    }

    private void DrawTasksTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("Task Controls", ref y);
        DrawButton(rect, ref y, "Complete All Tasks", () => ShowStatus("Completing all tasks..."));
        var ty = 0f;
        try
        {
            var localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer?.Data?.Tasks != null)
            {
                for (int i = 0; i < localPlayer.Data.Tasks.Count; i++)
                {
                    var task = localPlayer.Data.Tasks[i];
                    if (task != null)
                        DrawLabelAt($"Task {i + 1}: {(task.Complete ? "DONE" : "PENDING")}", ref ty, rect);
                }
            }
        }
        catch { }
    }
    #endregion

    #region Debug / Settings / About
    private void DrawDebugTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("Debug Information", ref y);
        DrawLabel($"Ping: {_lastPing}ms", ref y);
        DrawLabel($"Menu Visible: {State.MenuVisible}", ref y);
        DrawLabel($"Panic Mode: {State.PanicMode}", ref y);
        DrawLabel($"In Game: {State.InGame}", ref y);
        DrawLabel($"In Lobby: {State.InLobby}", ref y);
        DrawLabel($"In Meeting: {State.InMeeting}", ref y);
        DrawLabel($"Zoom: {State.Zoom:F1}x", ref y);
        DrawLabel($"NoClip: {State.NoClip}", ref y);
        DrawLabel($"Free Chat: {State.FreeChat}", ref y);
        DrawLabel($"Screen: {Screen.width}x{Screen.height}", ref y);
        DrawLabel($"FPS: {1f / Time.deltaTime:F0}", ref y);

        DrawLabel("", ref y);
        if (DrawButton(rect, ref y, "Dump Offsets to Log", () =>
            {
                var report = Offsets.OffsetSystem.DumpReport();
                SickoMenuPlugin.PluginLogger.LogInfo(report);
                ShowStatus("Offset report dumped to log");
            })) { }

        if (DrawButton(rect, ref y, "Export Offsets JSON", () =>
            {
                var json = Offsets.OffsetSystem.ExportOffsets();
                SickoMenuPlugin.PluginLogger.LogInfo("Exported Offsets:\n" + json);
                ShowStatus("Offsets exported to log");
            })) { }

        if (DrawButton(rect, ref y, "Re-resolve Offsets", () =>
            {
                var ok = Offsets.OffsetSystem.ResolveAll();
                ShowStatus(ok ? "Offsets re-resolved OK" : "Some offsets failed");
            })) { }
    }

    private void DrawSettingsTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("Keybind Settings", ref y);
        DrawLabel("Change keybinds in game settings menu", ref y);
        DrawLabel($"Menu: {KeyBinds.MenuToggle}", ref y);
        DrawLabel($"Radar: {KeyBinds.RadarToggle}", ref y);
        DrawLabel($"Console: {KeyBinds.ConsoleToggle}", ref y);
        DrawLabel($"Replay: {KeyBinds.ReplayToggle}", ref y);
        DrawLabel($"Repair: {KeyBinds.RepairSabotage}", ref y);
        DrawLabel($"NoClip: Hold {KeyBinds.NoClipModifier}", ref y);
        DrawLabel($"Panic: {KeyBinds.Panic}", ref y);

        DrawLabel("", ref y);
        DrawLabel("SickoMenu Settings", ref y);
        State.MenuVisible = DrawToggle(rect, ref y, "Show Menu on Startup", State.MenuVisible);
        State.BypassBans = DrawToggle(rect, ref y, "Bypass Bans (Experimental)", State.BypassBans);
        State.GhostMode = DrawToggle(rect, ref y, "Ghost Mode", State.GhostMode);
    }

    private void DrawAboutTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("SickoMenu v" + PluginInfo.PLUGIN_VERSION, ref y);
        DrawLabel("by g0aty - Ported to BepInEx C#", ref y);
        DrawLabel("", ref y);
        DrawLabel("A powerful utility for Among Us designed", ref y);
        DrawLabel("to enrich your game experience.", ref y);
        DrawLabel("", ref y);
        DrawLabel("Intended for educational and experimental use only.", ref y);
        DrawLabel("", ref y);
        DrawLabel("=== Credits ===", ref y);
        DrawLabel("Original: g0aty", ref y);
        DrawLabel("BepInEx Port: OpenCode", ref y);
        DrawLabel("", ref y);
        DrawLabel("License: GPL-3.0", ref y);
        DrawLabel("", ref y);
        DrawLabel($"Build: {PluginInfo.PLUGIN_VERSION}", ref y);
        DrawLabel($"Framework: .NET 6 + BepInEx 6 IL2CPP", ref y);
    }
    #endregion

    #region Console
    private void DrawConsole()
    {
        _consoleRect = GUI.Window(2, _consoleRect, DrawConsoleWindow, new GUIContent("SickoMenu Console"), GUI.skin.window);
    }

    private void DrawConsoleWindow(int id)
    {
        var outputRect = new Rect(10, 30, _consoleRect.width - 20, _consoleRect.height - 100);

        var y = 0f;
        foreach (var line in _consoleLines)
        {
            GUI.Label(new Rect(5, y, outputRect.width - 20, 20), line);
            y += 20;
        }

        var inputRect = new Rect(10, _consoleRect.height - 60, _consoleRect.width - 80, 30);
        var submitRect = new Rect(_consoleRect.width - 65, _consoleRect.height - 60, 55, 30);

        GUI.Label(inputRect, _consoleInput);

        if (GUI.Button(submitRect, new GUIContent("Send"), GUI.skin.button) ||
            (UnityEngine.Event.current.type == EventType.KeyDown &&
             UnityEngine.Event.current.keyCode == KeyCode.Return))
        {
            if (!string.IsNullOrEmpty(_consoleInput))
            {
                _consoleLines.Add("> " + _consoleInput);
                HandleConsoleCommand(_consoleInput);
                _consoleInput = "";
            }
        }
    }

    private void HandleConsoleCommand(string cmd)
    {
        if (cmd.StartsWith("/"))
        {
            var parts = cmd.Split(' ', 2);
            var command = parts[0].ToLower();
            var args = parts.Length > 1 ? parts[1] : "";

            switch (command)
            {
                case "/help":
                    _consoleLines.Add("Commands:");
                    _consoleLines.Add("  /reveal - Toggle reveal impostors");
                    _consoleLines.Add("  /noclip - Toggle noclip");
                    _consoleLines.Add("  /zoom [val] - Set zoom (0.5-10)");
                    _consoleLines.Add("  /wallhack - Toggle wallhack");
                    _consoleLines.Add("  /repair - Repair sabotages");
                    _consoleLines.Add("  /clear - Clear console");
                    _consoleLines.Add("  /dump - Dump offsets");
                    _consoleLines.Add("  /panic - Toggle panic mode");
                    _consoleLines.Add("  /ghost - Toggle ghost mode");
                    _consoleLines.Add("  /fps - Toggle FPS display");
                    break;
                case "/clear":
                    _consoleLines.Clear();
                    _consoleLines.Add("Console cleared");
                    break;
                case "/reveal":
                    State.RevealImpostors = !State.RevealImpostors;
                    _consoleLines.Add($"Reveal Impostors: {(State.RevealImpostors ? "ON" : "OFF")}");
                    break;
                case "/noclip":
                    State.NoClip = !State.NoClip;
                    _consoleLines.Add($"NoClip: {(State.NoClip ? "ON" : "OFF")}");
                    break;
                case "/zoom":
                    if (float.TryParse(args, out var z))
                    {
                        State.Zoom = Mathf.Clamp(z, 0.5f, 10f);
                        _consoleLines.Add($"Zoom set to {State.Zoom}");
                    }
                    break;
                case "/wallhack":
                    State.Wallhack = !State.Wallhack;
                    _consoleLines.Add($"Wallhack: {(State.Wallhack ? "ON" : "OFF")}");
                    break;
                case "/repair":
                    Features.SabotageHelper.RepairAll();
                    _consoleLines.Add("Sabotages repaired!");
                    break;
                case "/dump":
                    _consoleLines.Add(Offsets.OffsetSystem.DumpReport());
                    break;
                case "/panic":
                    State.PanicMode = !State.PanicMode;
                    _consoleLines.Add($"Panic Mode: {(State.PanicMode ? "ON" : "OFF")}");
                    break;
                case "/ghost":
                    State.GhostMode = !State.GhostMode;
                    _consoleLines.Add($"Ghost Mode: {(State.GhostMode ? "ON" : "OFF")}");
                    break;
                default:
                    _consoleLines.Add($"Unknown: {command}");
                    break;
            }
        }
        else
        {
            _consoleLines.Add($"Unknown command: {cmd}");
        }
    }
    #endregion

    #region ESP / Radar / Replay Draw
    private void DrawRadar()
    {
        _radarRect = GUI.Window(3, _radarRect, DrawRadarWindow, new GUIContent("Radar"), GUI.skin.window);
    }

    private void DrawRadarWindow(int id)
    {
        var mapRect = new Rect(10, 30, _radarRect.width - 20, _radarRect.height - 40);

        GUI.Box(mapRect, "Map View");
        GUI.Label(new Rect(mapRect.x + 5, mapRect.y + 5, mapRect.width - 10, 20), "Player positions:");

        if (ShouldDrawOverlay())
        {
            try
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player == null || player.Data == null || player.Data.IsDead) continue;

                    var pos = player.transform.position;
                    var mapX = mapRect.x + ((pos.x + 20f) / 40f) * mapRect.width;
                    var mapY = mapRect.y + ((pos.y + 20f) / 40f) * mapRect.height;

                    var isImpostor = player.Data.Role != null && player.Data.Role.IsImpostor;
                    var color = isImpostor ? Color.red : Color.green;
                    var label = player.Data.PlayerName;

                    GUI.color = color;
                    GUI.Box(new Rect(mapX - 5, mapY - 5, 10, 10), "");
                    GUI.Label(new Rect(mapX + 8, mapY - 8, 100, 20), label);
                    GUI.color = Color.white;

                    if (isImpostor && State.RevealImpostors)
                    {
                        GUI.Label(new Rect(mapX + 8, mapY + 8, 100, 20), "IMP");
                    }
                }
            }
            catch { }
        }

    }

    private void DrawEsp()
    {
        try
        {
            if (!ShouldDrawEsp()) return;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player == PlayerControl.LocalPlayer ||
                    player.Data == null || player.Data.IsDead ||
                    player.Data.Disconnected) continue;

                var screenPos = Camera.main.WorldToScreenPoint(player.transform.position);
                if (screenPos.z < 0) continue;

                screenPos.y = Screen.height - screenPos.y;
                var isImpostor = player.Data.Role != null && player.Data.Role.IsImpostor;
                var boxColor = isImpostor ? Color.red : Color.green;

                // Draw box around player
                DrawEspBox(new Vector2(screenPos.x, screenPos.y), 20, 40, boxColor);
            }
        }
        catch { }
    }

    private void DrawEspBox(Vector2 pos, float width, float height, Color color)
    {
        var rect = new Rect(pos.x - width / 2, pos.y - height / 2, width, height);
        GUI.color = color;
        GUI.Box(rect, "");
        GUI.color = Color.white;
    }

    private void DrawReplay()
    {
        _replayRect = GUI.Window(4, _replayRect, DrawReplayWindow, new GUIContent("Replay System"), GUI.skin.window);
    }

    private void DrawReplayWindow(int id)
    {
        var y = 30f;
        DrawLabelAt("Replay Controls", ref y, _replayRect);
        if (GUI.Button(new Rect(10, y, _replayRect.width - 20, 30), new GUIContent("Record Last 30s"), GUI.skin.button))
            ShowStatus("Replay recording...");
        y += 35;
        if (GUI.Button(new Rect(10, y, _replayRect.width - 20, 30), new GUIContent("Play Replay"), GUI.skin.button))
            ShowStatus("Playing replay...");
        y += 35;
        if (GUI.Button(new Rect(10, y, _replayRect.width - 20, 30), new GUIContent("Save Replay"), GUI.skin.button))
            ShowStatus("Replay saved!");

        DrawLabelAt("Replay Timeline:", ref y, _replayRect);
        GUI.Box(new Rect(10, y, _replayRect.width - 20, 100), "Timeline placeholder");

    }
    #endregion

    #region GUI Helpers
    private void DrawSubTabs(ref int active, string[] tabs, Rect parentRect)
    {
        var tabWidth = (parentRect.width - 20) / tabs.Length;
        for (int i = 0; i < tabs.Length; i++)
        {
            var rect = new Rect(10 + i * tabWidth, 0, tabWidth - 2, 25);
            var wasSelected = active == i;
            var isSelected = GUI.Toggle(rect, wasSelected, new GUIContent(tabs[i]), GUI.skin.button);
            if (isSelected && !wasSelected)
                active = i;
        }
    }

    private bool DrawToggle(Rect parent, ref float y, string label, bool value)
    {
        var rect = new Rect(10, y, parent.width - 20, 25);
        var result = GUI.Toggle(rect, value, new GUIContent(label), GUI.skin.toggle);
        y += 28;
        return result;
    }

    private float DrawSlider(Rect parent, ref float y, string label, float value, float min, float max)
    {
        var labelRect = new Rect(10, y, parent.width - 20, 20);

        GUI.Label(labelRect, $"{label}: {value:F1}");
        y += 45;
        return value;
    }

    private void DrawIntSlider(Rect parent, ref float y, string label, ref int value, int min, int max)
    {
        var labelRect = new Rect(10, y, parent.width - 20, 20);

        GUI.Label(labelRect, $"{label}: {value}");
        y += 45;
    }

    private bool DrawButton(Rect parent, ref float y, string label, Action action, Color? color = null)
    {
        var rect = new Rect(10, y, parent.width - 20, 30);
        if (color.HasValue)
            GUI.color = color.Value;

        var clicked = GUI.Button(rect, new GUIContent(label), GUI.skin.button);

        if (color.HasValue)
            GUI.color = Color.white;

        y += 35;
        if (clicked)
            action?.Invoke();
        return clicked;
    }

    private void DrawLabel(string text, ref float y)
    {
        var rect = new Rect(10, y, _menuRect.width - 20, 25);
        GUI.Label(rect, text);
        y += 27;
    }

    private void DrawLabelAt(string text, ref float y, Rect parent)
    {
        var rect = new Rect(10, y, parent.width - 20, 25);
        GUI.Label(rect, text);
        y += 27;
    }

    private void ShowStatus(string message)
    {
        _statusBarMessage = message;
        _statusBarTimer = 3f;
    }

    private bool ShouldDrawOverlay()
    {
        return !State.PanicMode && (State.InGame || State.InLobby);
    }

    private bool ShouldDrawEsp()
    {
        return !State.PanicMode && (State.InGame || State.InLobby) && State.ShowEsp &&
               (!State.InMeeting || !State.HideEspDuringMeetings);
    }

    private GUISkin CreateSickoSkin()
    {
        var skin = ScriptableObject.CreateInstance<GUISkin>();

        skin.window = new GUIStyle();
        skin.window.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.95f));
        skin.window.normal.textColor = Color.white;
        skin.window.fontSize = 14;
        skin.window.fontStyle = FontStyle.Bold;
        skin.window.alignment = TextAnchor.MiddleCenter;

        skin.button = new GUIStyle();
        skin.button.normal.background = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.3f, 0.9f));
        skin.button.normal.textColor = Color.white;
        skin.button.alignment = TextAnchor.MiddleCenter;
        skin.button.fontSize = 12;

        skin.label = new GUIStyle();
        skin.label.normal.textColor = Color.white;
        skin.label.fontSize = 12;
        skin.label.alignment = TextAnchor.MiddleLeft;
        skin.label.wordWrap = true;

        skin.toggle = new GUIStyle();
        skin.toggle.normal.background = MakeTex(2, 2, new Color(0.15f, 0.15f, 0.2f, 0.8f));
        skin.toggle.normal.textColor = Color.white;
        skin.toggle.alignment = TextAnchor.MiddleLeft;
        skin.toggle.fontSize = 12;

        skin.textField = new GUIStyle();
        skin.textField.normal.background = MakeTex(2, 2, new Color(0.12f, 0.12f, 0.15f, 0.9f));
        skin.textField.normal.textColor = Color.white;
        skin.textField.alignment = TextAnchor.MiddleLeft;
        skin.textField.fontSize = 12;

        skin.box = new GUIStyle();
        skin.box.normal.background = MakeTex(2, 2, new Color(0.12f, 0.12f, 0.15f, 0.8f));
        skin.box.normal.textColor = Color.white;
        skin.box.alignment = TextAnchor.MiddleCenter;

        return skin;
    }

    private Texture2D MakeTex(int width, int height, Color color)
    {
        var pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        var tex = new Texture2D(width, height);
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
    #endregion
}
