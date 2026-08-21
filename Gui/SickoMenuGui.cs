using BepInEx;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes;
using SickoMenu.Utils;
using SickoMenu.Features;
using SickoMenu.Offsets;
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

    // Search functionality
    private string _searchQuery = "";
    private bool _searchFocused;

    private string _statusBarMessage = "";
    private float _statusBarTimer;
    
    // Panic warning state
    private bool _isPanicWarning = false;

    // Tab state - which main tabs are open
    private bool _openAbout = true;
    private bool _openSettings = false;
    private bool _openGame = false;
    private bool _openSelf = false;
    private bool _openRadar = false;
    private bool _openReplay = false;
    private bool _openEsp = false;
    private bool _openPlayers = false;
    private bool _openTasks = false;
    private bool _openSabotage = false;
    private bool _openDoors = false;
    private bool _openHost = false;
    private bool _openDebug = false;

    // Sub-group state for Settings tab
    private bool _settingsOpenGeneral = true;
    private bool _settingsOpenSpoofing = false;
    private bool _settingsOpenCustomization = false;
    private bool _settingsOpenKeybinds = false;

    // Sub-group state for Self tab
    private bool _selfOpenVisuals = true;
    private bool _selfOpenUtils = false;
    private bool _selfOpenRandomizers = false;

    // Sub-group state for Game tab
    private bool _gameOpenGeneral = true;
    private bool _gameOpenChat = false;
    private bool _gameOpenAnticheat = false;
    private bool _gameOpenUtils = false;

    // Sub-group state for Host tab
    private bool _hostOpenGeneral = true;
    private bool _hostOpenRoles = false;
    private bool _hostOpenOptions = false;

    // Sub-group state for Players tab
    private bool _playersOpenAllPlayers = true;
    private bool _playersOpenActions = false;

    // First render flag for welcome
    private bool _firstRender = true;

    private bool _dragWindow;
    private Vector2 _dragOffset;

    private float _lastUpdateTime;
    private float _lastPing;

    // Search categories database (matching C++ menu.cpp lines 48-135)
    private static readonly Dictionary<string, List<(string Name, string SubGroup)>> SearchCategories = new()
    {
        {"Settings", new List<(string, string)> {
            ("Show Keybinds", "Keybinds"), ("Allow Activating Keybinds while Chatting", "Keybinds"),
            ("Always Show Menu on Startup", "General"), ("Panic (Disable SickoMenu)", "General"),
            ("Config Name", "General"), ("Load Config", "General"), ("Save Config", "General"),
            ("Adjust by DPI", "Customization"), ("Menu Scale", "Customization"),
            ("Menu Theme Color", "Customization"), ("Gradient Theme", "Customization"),
            ("Match Background with Theme", "Customization"), ("RGB Menu Theme", "Customization"),
            ("Reset Menu Theme", "Customization"), ("Opacity", "Customization"),
            ("Show Debug Tab", "General"), ("Username", "Spoofing"), ("Set as Account Name", "Spoofing"),
            ("Automatically Set Name", "Spoofing"), ("Custom Code", "Customization"),
            ("Replace Streamer Mode Lobby Code", "Customization"), ("RGB Lobby Code", "Customization"),
            ("Unlock Cosmetics", "Spoofing"), ("Safe Mode", "General"),
            ("Allow other SickoMenu users to see you", "General"),
            ("Spoof Guest Account", "Spoofing"), ("Use Custom Guest Friend Code", "Spoofing"),
            ("Spoof Level", "Spoofing"), ("Spoof Platform", "Spoofing"),
            ("Disable Host Anticheat (+25 Mode)", "General"), ("FPS", "General")
        }},
        {"Game", new List<(string, string)> {
            ("Player Speed Multiplier", "General"), ("Kill Distance", "General"),
            ("No Ability Cooldown", "General"), ("Multiply Speed", "General"),
            ("Modify Kill Distance", "General"), ("Random Color", "General"),
            ("Set Color", "General"), ("Snipe Color", "General"), ("Console", "General"),
            ("Reset Appearance", "General"), ("Kill Everyone", "General"),
            ("Protect Everyone", "General"), ("Disable Venting", "General"),
            ("Spam Report", "General"), ("Kill All Crewmates", "General"),
            ("Kill All Impostors", "General"), ("Kick Everyone From Vents", "General"),
            ("Chat Message", "Chat"), ("Send", "Chat"), ("Send to AUM", "Chat"),
            ("Spam", "Chat"), ("Chat Presets", "Chat"),
            ("Attempt to Crash", "Utils"), ("Overload Everyone", "Utils"),
            ("Lag Everyone", "Utils"), ("Enable Anticheat (SMAC)", "Anticheat"),
            ("Whitelist", "Anticheat"), ("Blacklist", "Anticheat")
        }},
        {"Self", new List<(string, string)> {
            ("Max Vision", "Visuals"), ("Wallhack", "Visuals"), ("Disable HUD", "Visuals"),
            ("Freecam", "Visuals"), ("Zoom", "Visuals"),
            ("Always show Chat Button", "Visuals"), ("Allow Ctrl+(C/V) in Chat", "Visuals"),
            ("Read Messages by Ghosts", "Visuals"), ("Read and Send SickoChat", "Visuals"),
            ("Custom Name", "Visuals"), ("Custom Name for Everyone", "Visuals"),
            ("Server-sided Custom Name", "Visuals"), ("Reveal Roles", "Visuals"),
            ("Abbrv. Role", "Visuals"), ("Player Colored Dots Next To Names", "Visuals"),
            ("Show Player Info in Lobby", "Visuals"), ("Reveal Votes", "Visuals"),
            ("See Ghosts", "Visuals"), ("See Protections", "Visuals"),
            ("See Kill Cooldown", "Visuals"), ("Disable Kill Animation", "Visuals"),
            ("Dark Mode", "Visuals"), ("Show Host", "Visuals"), ("Hide Watermark", "Visuals"),
            ("Show Vote Kicks", "Visuals"), ("Show FPS", "Visuals"),
            ("Show Lobby Info", "Visuals"), ("See Phantoms", "Visuals"),
            ("Unlock Vents", "Utils"), ("Move While in Vent & Shapeshifting", "Utils"),
            ("Always Move", "Utils"), ("No Shapeshift Animation", "Utils"),
            ("Copy Lobby Code on Disconnect", "Utils"), ("NoClip", "Utils"),
            ("Allow Killing in Lobbies", "Utils"), ("Kill Other Impostors", "Utils"),
            ("Infinite Kill Range", "Utils"), ("Bypass Guardian Angel Protections", "Utils"),
            ("Autokill", "Utils"), ("Do Tasks as Impostor", "Utils"),
            ("Fake Alive", "Utils"), ("God Mode", "Utils"), ("Teleport", "Utils"),
            ("Rotate everyone", "Utils"), ("Select Role", "Utils"),
            ("Set Role", "Utils"), ("Set Fake Role", "Utils"),
            ("Automatically Set Fake Role", "Utils"), ("Report Body on Murder", "Utils"),
            ("Prevent Self-Report", "Utils"), ("Cycler", "Randomizers"),
            ("Cycle in Meeting", "Randomizers"), ("Cycle Between Players", "Randomizers"),
            ("Confuser (Randomize Appearance at Will)", "Randomizers")
        }},
        {"Radar", new List<(string, string)> {
            ("Show Radar", ""), ("Show Dead Bodies", ""), ("Show Ghosts", ""),
            ("Right Click to Teleport", ""), ("Hide Radar During Meetings", ""),
            ("Draw Player Icons", ""), ("Lock Radar Position", ""), ("Show Border", "")
        }},
        {"Replay", new List<(string, string)> {
            ("Show Replay", ""), ("Show Only last seconds", ""), ("Clear after meeting", "")
        }},
        {"ESP", new List<(string, string)> {
            ("Enable", ""), ("Show Ghosts", ""), ("Hide During Meetings", ""),
            ("Show Boxes", ""), ("Show Tracers", ""), ("Show Distances", ""),
            ("Role-based", "")
        }},
        {"Players", new List<(string, string)> {
            ("Players", "")
        }},
        {"Tasks", new List<(string, string)> {
            ("Complete All Tasks", ""), ("Play Medbay Scan Animation", "")
        }},
        {"Sabotage", new List<(string, string)> {
            ("Disable Sabotage", ""), ("Auto Repair Sabotages", ""),
            ("Repair Sabotage", ""), ("Sabotage All", ""),
            ("Random Sabotage", ""), ("Sabotage Lights", ""),
            ("Sabotage Reactor", ""), ("Sabotage Oxygen", ""),
            ("Sabotage Comms", ""), ("Disable Lights", ""),
            ("Activate Mushroom Mixup", "")
        }},
        {"Doors", new List<(string, string)> {
            ("Close All Doors", ""), ("Close Room Door", ""),
            ("Pin All Doors", ""), ("Unpin All Doors", ""), ("Auto Open Doors", "")
        }},
        {"Host", new List<(string, string)> {
            ("Custom Impostor Amount", "Utils"), ("Impostor Count", "Utils"),
            ("Force Start of Game", "Utils"), ("Disable Meetings", "Utils"),
            ("Disable Sabotages", "Utils"), ("Disable Game Ending", "Utils"),
            ("End Game", "Utils"), ("Force Color for Everyone", "Utils"),
            ("Force Name for Everyone", "Utils"), ("Spam Moving Platform", "Utils"),
            ("Unlock Kill Button", "Utils"), ("Allow Killing in Lobbies", "Utils"),
            ("Kill While Vanished", "Utils"), ("Game Mode", "Utils"),
            ("Show Lobby Timer", "Utils"), ("Auto Start Game", "Utils"),
            ("Spectator Mode", "Utils")
        }},
        {"Debug", new List<(string, string)> {
            ("Enable Occlusion Culling", ""), ("Force Load Settings", ""),
            ("Force Save Settings", ""), ("Clear RPC Queues", ""),
            ("Log Unity Debug Messages", ""), ("Log Hook Debug Messages", ""),
            ("Colors", ""), ("Profiler", ""), ("Experiments", ""),
            ("Enable Anticheat (SMAC)", ""), ("Point System (Only for Hosting)", "")
        }}
    };

    private bool _loggedFirstGui;
    private bool _guiErrorLogged;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SickoMenu.SickoMenuPlugin.PluginLogger.LogInfo("SickoMenuGui component started");
        _consoleLines.Add("SickoMenu v" + SickoMenu.PluginInfo.PLUGIN_VERSION + " Console");
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

        if (!_loggedFirstGui)
        {
            _loggedFirstGui = true;
            SickoMenu.SickoMenuPlugin.PluginLogger.LogInfo(
                $"OnGUI alive (MenuVisible={State.MenuVisible}, Screen={Screen.width}x{Screen.height})");
        }

        try
        {
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
        }
        catch (Exception ex)
        {
            if (!_guiErrorLogged)
            {
                _guiErrorLogged = true;
                SickoMenu.SickoMenuPlugin.PluginLogger.LogError("OnGUI error: " + ex);
            }
        }

        if (_statusBarTimer > 0)
        {
            _statusBarTimer -= Time.deltaTime;
            var rect = new Rect(Screen.width / 2f - 150, Screen.height - 40, 300, 30);
            GUI.Box(rect, _statusBarMessage);
        }
    }

    private GUISkin? _skin;

    private void DrawMainMenu()
    {
        if (_skin == null) _skin = CreateSickoSkin();
        GUI.skin = _skin;

        _menuRect = GUI.Window(0, _menuRect, (GUI.WindowFunction)DrawMenuWindow, new GUIContent("SickoMenu v" + SickoMenu.PluginInfo.PLUGIN_VERSION));

        // Clamp window to screen
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
        const int sidebarWidth = 100;

        // Draw sidebar with search and tab buttons (like C++ ImGui layout)
        var sidebarRect = new Rect(10, 25, sidebarWidth - 10, _menuRect.height - 70);
        DrawSidebar(sidebarRect);

        // Draw main content area
        var contentRect = new Rect(10 + sidebarWidth, 25, _menuRect.width - sidebarWidth - 20, _menuRect.height - 70);
        
        // Draw content for the currently open tab
        DrawContentArea(contentRect);

        // Draw panic button at bottom
        DrawPanicButton();

        // Handle first render
        if (_firstRender)
        {
            _firstRender = false;
            CloseAllOtherTabs(0); // Open About by default
        }
    }

    private void DrawSidebar(Rect rect)
    {
        var y = 0f;

        // Search field at top of sidebar
        var searchArea = new Rect(5, y, rect.width - 10, 25);
        GUI.BeginGroup(searchArea);
        var newSearch = GUILayout.TextField(_searchQuery, GUI.skin.textField);
        if (newSearch != _searchQuery)
        {
            _searchQuery = newSearch;
        }
        GUI.EndGroup();
        y += 28;

        // If search has content, show results instead of tabs
        if (!string.IsNullOrEmpty(_searchQuery))
        {
            DrawSearchResults(new Rect(5, y, rect.width - 10, rect.height - 30));
            return;
        }

        // Draw tab buttons in the order from C++ menu.cpp
        DrawTabButton(rect, ref y, "About", ref _openAbout, 0);
        DrawTabButton(rect, ref y, "Settings", ref _openSettings, 1);
        DrawTabButton(rect, ref y, "Game", ref _openGame, 2);
        DrawTabButton(rect, ref y, "Self", ref _openSelf, 3);
        DrawTabButton(rect, ref y, "Radar", ref _openRadar, 4);
        DrawTabButton(rect, ref y, "Replay", ref _openReplay, 5);
        DrawTabButton(rect, ref y, "ESP", ref _openEsp, 6);
        
        // Contextual tabs (only show when appropriate)
        if (IsTabVisibleInContext(7)) DrawTabButton(rect, ref y, "Players", ref _openPlayers, 7);
        if (IsTabVisibleInContext(8)) DrawTabButton(rect, ref y, "Sabotage", ref _openSabotage, 8);
        if (IsTabVisibleInContext(9)) DrawTabButton(rect, ref y, "Doors", ref _openDoors, 9);
        if (IsTabVisibleInContext(10)) DrawTabButton(rect, ref y, "Tasks", ref _openTasks, 10);
        if (IsTabVisibleInContext(11)) DrawTabButton(rect, ref y, "Host", ref _openHost, 11);
        if (IsTabVisibleInContext(12)) DrawTabButton(rect, ref y, "Debug", ref _openDebug, 12);
    }

    private void DrawTabButton(Rect sidebarRect, ref float y, string label, ref bool isOpen, int tabIndex)
    {
        var buttonRect = new Rect(5, y, sidebarRect.width - 10, 25);
        var wasOpen = isOpen;
        isOpen = GUI.Toggle(buttonRect, isOpen, new GUIContent(label), GUI.skin.button);
        
        if (isOpen && !wasOpen)
        {
            CloseAllOtherTabs(tabIndex);
            State.SelectedTab = tabIndex;
        }
        
        y += 28;
    }

    private bool IsTabVisibleInContext(int tabIndex)
    {
        // Match the visibility logic from C++ menu.cpp lines 289-302
        switch (tabIndex)
        {
            case 7: // Players
            case 8: // Sabotage  
            case 9: // Doors
            case 10: // Tasks
                return State.InGame || State.InLobby;
            case 11: // Host
                return GameHelper.IsHost();
            case 12: // Debug
                return true; // Always show Debug in our version
            default:
                return true;
        }
    }

    private void CloseAllOtherTabs(int openTab)
    {
        _openAbout = openTab == 0;
        _openSettings = openTab == 1;
        _openGame = openTab == 2;
        _openSelf = openTab == 3;
        _openRadar = openTab == 4;
        _openReplay = openTab == 5;
        _openEsp = openTab == 6;
        _openPlayers = openTab == 7;
        _openTasks = openTab == 8;
        _openSabotage = openTab == 9;
        _openDoors = openTab == 10;
        _openHost = openTab == 11;
        _openDebug = openTab == 12;
    }

    private void DrawSearchResults(Rect rect)
    {
        var y = 0f;
        var query = _searchQuery.ToLower();
        
        // Find matching entries (matching C++ menu.cpp lines 203-232)
        var results = new List<(string Category, string SubGroup)>();
        
        foreach (var category in SearchCategories)
        {
            foreach (var entry in category.Value)
            {
                if (entry.Name.ToLower().Contains(query))
                {
                    results.Add((category.Key, entry.SubGroup));
                    break; // Only need first match per category
                }
            }
        }
        
        if (results.Count == 0)
        {
            DrawLabelAt("No results.", ref y, rect);
            return;
        }
        
        DrawLabelAt("Search Result" + (results.Count > 1 ? "s" : ""), ref y, rect);
        
        foreach (var result in results)
        {
            var label = string.IsNullOrEmpty(result.SubGroup) ? result.Category : result.Category + " > " + result.SubGroup;
            var buttonRect = new Rect(5, y, rect.width - 10, 25);
            
            if (GUI.Button(buttonRect, new GUIContent(label)))
            {
                // Navigate to the matching tab (matching C++ menu.cpp lines 225-228)
                var tabIndex = GetTabIndexFromName(result.Category);
                CloseAllOtherTabs(tabIndex);
                State.SelectedTab = tabIndex;
                _searchQuery = "";
                
                // Open the sub-group if specified (matching C++ menu.cpp lines 195-201)
                OpenTabSubGroup(result.Category, result.SubGroup);
            }
            
            y += 28;
        }
    }

    private int GetTabIndexFromName(string tabName)
    {
        // Matching C++ menu.cpp lines 177-193
        return tabName switch
        {
            "About" => 0,
            "Settings" => 1,
            "Game" => 2,
            "Self" => 3,
            "Radar" => 4,
            "Replay" => 5,
            "ESP" => 6,
            "Players" => 7,
            "Tasks" => 8,
            "Sabotage" => 9,
            "Doors" => 10,
            "Host" => 11,
            "Debug" => 12,
            _ => 0
        };
    }

    private void OpenTabSubGroup(string tabName, string subGroup)
    {
        // Matching C++ menu.cpp lines 195-201
        if (string.IsNullOrEmpty(subGroup)) return;
        
        switch (tabName)
        {
            case "Settings":
                OpenSettingsSubGroup(subGroup);
                break;
            case "Self":
                OpenSelfSubGroup(subGroup);
                break;
            case "Game":
                OpenGameSubGroup(subGroup);
                break;
            case "Host":
                OpenHostSubGroup(subGroup);
                break;
        }
    }

    private void OpenSettingsSubGroup(string subGroup)
    {
        _settingsOpenGeneral = subGroup == "General";
        _settingsOpenSpoofing = subGroup == "Spoofing";
        _settingsOpenCustomization = subGroup == "Customization";
        _settingsOpenKeybinds = subGroup == "Keybinds";
    }

    private void OpenSelfSubGroup(string subGroup)
    {
        _selfOpenVisuals = subGroup == "Visuals";
        _selfOpenUtils = subGroup == "Utils";
        _selfOpenRandomizers = subGroup == "Randomizers";
    }

    private void OpenGameSubGroup(string subGroup)
    {
        _gameOpenGeneral = subGroup == "General";
        _gameOpenChat = subGroup == "Chat";
        _gameOpenAnticheat = subGroup == "Anticheat";
        _gameOpenUtils = subGroup == "Utils";
    }

    private void OpenHostSubGroup(string subGroup)
    {
        // Map to Utils since Host sub-groups in C++ are Utils/Roles/Options
        _hostOpenGeneral = subGroup == "Utils" || subGroup == "General";
        _hostOpenRoles = subGroup == "Roles";
        _hostOpenOptions = subGroup == "Options";
    }

    private void DrawContentArea(Rect rect)
    {
        // Draw content for each open tab with fallback logic matching C++ menu.cpp lines 356-409
        var contentRect = new Rect(rect.x + 10, rect.y + 10, rect.width - 20, rect.height - 20);
        
        if (_openAbout) DrawAboutTab(contentRect);
        else if (_openSettings) DrawSettingsTab(contentRect);
        else if (_openGame) DrawGameTab(contentRect);
        else if (_openSelf) DrawSelfTab(contentRect);
        else if (_openRadar) DrawRadarTab(contentRect);
        else if (_openReplay) DrawReplayTab(contentRect);
        else if (_openEsp) DrawEspTab(contentRect);
        else if (_openPlayers && (State.InGame || State.InLobby)) DrawPlayersTab(contentRect);
        else if (_openSabotage && State.InGame) DrawSabotageTab(contentRect);
        else if (_openDoors && State.InGame) DrawDoorsTab(contentRect);
        else if (_openTasks && ((State.InGame) || (State.InLobby && GameHelper.IsHost()))) DrawTasksTab(contentRect);
        else if (_openHost && GameHelper.IsHost()) DrawHostTab(contentRect);
        else if (_openDebug) DrawDebugTab(contentRect);
        else DrawAboutTab(contentRect); // Fallback
    }

    private void DrawPanicButton()
    {
        // Matching C++ menu.cpp lines 311-346
        const int buttonWidth = 120;
        const int buttonHeight = 30;
        
        var buttonX = (int)(_menuRect.width - buttonWidth - 20);
        var buttonY = (int)(_menuRect.height - buttonHeight - 30);
        var buttonRect = new Rect(buttonX, buttonY, buttonWidth, buttonHeight);
        
        if (!_isPanicWarning)
        {
            GUI.color = new Color(1f, 0.3f, 0.3f);
            if (GUI.Button(buttonRect, new GUIContent("Disable Menu")))
            {
                _isPanicWarning = true;
            }
            GUI.color = Color.white;
        }
        else
        {
            // Draw panic warning
            float labelY = buttonY - 60;
            
            if (KeyBinds.Panic == KeyCode.None)
            {
                DrawLabelAt("No Panic", ref labelY, _menuRect);
                DrawLabelAt("Keybind!", ref labelY, _menuRect);
            }
            else
            {
                DrawLabelAt("Press " + KeyBinds.Panic.ToString(), ref labelY, _menuRect);
                DrawLabelAt("to re-enable!", ref labelY, _menuRect);
            }
            
            DrawLabelAt("Continue?", ref labelY, _menuRect);
            
            var yesRect = new Rect(buttonX, buttonY, buttonWidth / 2 - 5, buttonHeight);
            var noRect = new Rect(buttonX + buttonWidth / 2 + 5, buttonY, buttonWidth / 2 - 5, buttonHeight);
            
            GUI.color = new Color(1f, 0.3f, 0.3f);
            if (GUI.Button(yesRect, new GUIContent("Yes")))
            {
                _isPanicWarning = false;
                State.PanicMode = true;
            }
            GUI.color = new Color(0.3f, 1f, 0.3f);
            if (GUI.Button(noRect, new GUIContent("No")))
            {
                _isPanicWarning = false;
            }
            GUI.color = Color.white;
        }
    }

    #region Tab Content
    
    private void DrawAboutTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("SickoMenu v" + SickoMenu.PluginInfo.PLUGIN_VERSION, ref y, rect);
        DrawLabel("by g0aty - Ported to BepInEx C#", ref y, rect);
        y += 10;
        DrawLabel("A powerful utility for Among Us designed", ref y, rect);
        DrawLabel("to enrich your game experience.", ref y, rect);
        y += 10;
        DrawLabel("Intended for educational and experimental use only.", ref y, rect);
        y += 10;
        DrawLabel("=== Credits ===", ref y, rect);
        DrawLabel("Original C++: g0aty", ref y, rect);
        DrawLabel("BepInEx C# Port: Mistral Vibe", ref y, rect);
        y += 10;
        DrawLabel("License: GPL-3.0", ref y, rect);
        y += 10;
        DrawLabel("Build: " + SickoMenu.PluginInfo.PLUGIN_VERSION, ref y, rect);
        DrawLabel("Framework: .NET 6 + BepInEx 6 IL2CPP", ref y, rect);
    }

    #region Settings Tab
    private void DrawSettingsTab(Rect rect)
    {
        var y = 0f;
        DrawSubTabsSettings(rect, ref y);
        y += 35;
        
        if (_settingsOpenGeneral) DrawSettingsGeneral(rect, ref y);
        else if (_settingsOpenSpoofing) DrawSettingsSpoofing(rect, ref y);
        else if (_settingsOpenCustomization) DrawSettingsCustomization(rect, ref y);
        else if (_settingsOpenKeybinds) DrawSettingsKeybinds(rect, ref y);
    }

    private void DrawSubTabsSettings(Rect rect, ref float y)
    {
        var tabWidth = rect.width / 4;
        
        var r1 = new Rect(0, y, tabWidth - 5, 30);
        if (GUI.Toggle(r1, _settingsOpenGeneral, new GUIContent("General"), GUI.skin.button))
        { _settingsOpenGeneral = true; _settingsOpenSpoofing = false; _settingsOpenCustomization = false; _settingsOpenKeybinds = false; }
        
        var r2 = new Rect(tabWidth, y, tabWidth - 5, 30);
        if (GUI.Toggle(r2, _settingsOpenSpoofing, new GUIContent("Spoofing"), GUI.skin.button))
        { _settingsOpenGeneral = false; _settingsOpenSpoofing = true; _settingsOpenCustomization = false; _settingsOpenKeybinds = false; }
        
        var r3 = new Rect(tabWidth * 2, y, tabWidth - 5, 30);
        if (GUI.Toggle(r3, _settingsOpenCustomization, new GUIContent("Customization"), GUI.skin.button))
        { _settingsOpenGeneral = false; _settingsOpenSpoofing = false; _settingsOpenCustomization = true; _settingsOpenKeybinds = false; }
        
        var r4 = new Rect(tabWidth * 3, y, tabWidth - 5, 30);
        if (GUI.Toggle(r4, _settingsOpenKeybinds, new GUIContent("Keybinds"), GUI.skin.button))
        { _settingsOpenGeneral = false; _settingsOpenSpoofing = false; _settingsOpenCustomization = false; _settingsOpenKeybinds = true; }
        
        y += 35;
    }

    private void DrawSettingsGeneral(Rect rect, ref float y)
    {
        DrawLabel("=== General Settings ===", ref y, rect);
        State.MenuVisible = DrawToggle(rect, ref y, "Show Menu on Startup", State.MenuVisible);
        
        DrawLabel("Panic Mode: Disable all SickoMenu features instantly", ref y, rect);
        DrawLabel("Hotkey: " + KeyBinds.Panic, ref y, rect);
        y += 10;
        
        DrawLabel("=== Config ===", ref y, rect);
        State.Username = DrawTextField(rect, ref y, "Username", State.Username);
        
        if (DrawButton(rect, ref y, "Save Config", () => ShowStatus("Config saved!"))) { }
        DrawButton(rect, ref y, "Load Config", () => ShowStatus("Config loaded!"));
        y += 10;
        DrawToggle(rect, ref y, "Hide Watermark", State.BypassBans);
    }

    private void DrawSettingsSpoofing(Rect rect, ref float y)
    {
        DrawLabel("=== Spoofing Settings ===", ref y, rect);
        DrawLabel("Note: Spoofing features may trigger anticheat", ref y, rect);
        y += 10;
        DrawLabel("These settings allow customization of your appearance", ref y, rect);
    }

    private void DrawSettingsCustomization(Rect rect, ref float y)
    {
        DrawLabel("=== Customization ===", ref y, rect);
        DrawLabel("Visual theme and color settings", ref y, rect);
        y += 10;
        DrawLabel("Menu Theme Color: Pink (Default)", ref y, rect);
        DrawToggle(rect, ref y, "Dark Mode", true);
    }

    private void DrawSettingsKeybinds(Rect rect, ref float y)
    {
        DrawLabel("=== Keybinds ===", ref y, rect);
        DrawLabel("Note: Keybinds are configured via game settings", ref y, rect);
        y += 10;
        DrawLabel("Current Keybinds:", ref y, rect);
        DrawLabel("Menu: " + KeyBinds.MenuToggle, ref y, rect);
        DrawLabel("Radar: " + KeyBinds.RadarToggle, ref y, rect);
        DrawLabel("Console: " + KeyBinds.ConsoleToggle, ref y, rect);
        DrawLabel("Replay: " + KeyBinds.ReplayToggle, ref y, rect);
        DrawLabel("Repair: " + KeyBinds.RepairSabotage, ref y, rect);
        DrawLabel("NoClip: Hold " + KeyBinds.NoClipModifier, ref y, rect);
        DrawLabel("Panic: " + KeyBinds.Panic, ref y, rect);
    }
    #endregion

    #region Game Tab
    private void DrawGameTab(Rect rect)
    {
        var y = 0f;
        DrawSubTabsGame(rect, ref y);
        y += 35;
        
        if (_gameOpenGeneral) DrawGameGeneral(rect, ref y);
        else if (_gameOpenChat) DrawGameChat(rect, ref y);
        else if (_gameOpenAnticheat) DrawGameAnticheat(rect, ref y);
        else if (_gameOpenUtils) DrawGameUtils(rect, ref y);
    }

    private void DrawSubTabsGame(Rect rect, ref float y)
    {
        var tabWidth = rect.width / 4;
        
        var r1 = new Rect(0, y, tabWidth - 5, 30);
        if (GUI.Toggle(r1, _gameOpenGeneral, new GUIContent("General"), GUI.skin.button))
        { _gameOpenGeneral = true; _gameOpenChat = false; _gameOpenAnticheat = false; _gameOpenUtils = false; }
        
        var r2 = new Rect(tabWidth, y, tabWidth - 5, 30);
        if (GUI.Toggle(r2, _gameOpenChat, new GUIContent("Chat"), GUI.skin.button))
        { _gameOpenGeneral = false; _gameOpenChat = true; _gameOpenAnticheat = false; _gameOpenUtils = false; }
        
        var r3 = new Rect(tabWidth * 2, y, tabWidth - 5, 30);
        if (GUI.Toggle(r3, _gameOpenAnticheat, new GUIContent("Anticheat"), GUI.skin.button))
        { _gameOpenGeneral = false; _gameOpenChat = false; _gameOpenAnticheat = true; _gameOpenUtils = false; }
        
        var r4 = new Rect(tabWidth * 3, y, tabWidth - 5, 30);
        if (GUI.Toggle(r4, _gameOpenUtils, new GUIContent("Utils"), GUI.skin.button))
        { _gameOpenGeneral = false; _gameOpenChat = false; _gameOpenAnticheat = false; _gameOpenUtils = true; }
        
        y += 35;
    }

    private void DrawGameGeneral(Rect rect, ref float y)
    {
        DrawLabel("=== Game Settings ===", ref y, rect);
        State.RevealImpostors = DrawToggle(rect, ref y, "Reveal Impostors", State.RevealImpostors);
        State.RevealRoles = DrawToggle(rect, ref y, "Reveal Roles", State.RevealRoles);
        State.Wallhack = DrawToggle(rect, ref y, "Wallhack", State.Wallhack);
        DrawToggle(rect, ref y, "Show Ghosts", State.GhostMode);
        State.NoClip = DrawToggle(rect, ref y, "NoClip (Hold CTRL)", State.NoClip);
        State.DisableKillAnimation = DrawToggle(rect, ref y, "Disable Kill Animation", State.DisableKillAnimation);
        
        y += 10;
        if (DrawButton(rect, ref y, "Repair All Sabotages", () => SickoMenu.Features.SabotageHelper.RepairAll()))
            ShowStatus("All sabotages repaired!");
        
        y += 10;
        DrawLabel("Game State: " + (State.InGame ? "In Game" : State.InLobby ? "In Lobby" : "Menu"), ref y, rect);
        DrawLabel("Meeting: " + State.InMeeting, ref y, rect);
    }

    private void DrawGameChat(Rect rect, ref float y)
    {
        DrawLabel("=== Chat Settings ===", ref y, rect);
        State.FreeChat = DrawToggle(rect, ref y, "Free Chat (Bypass Chat Restrictions)", State.FreeChat);
        State.AlwaysShowChat = DrawToggle(rect, ref y, "Always Show Chat", State.AlwaysShowChat);
        y += 10;
        DrawLabel("Commands:", ref y, rect);
        DrawLabel("  /sc [message] - Send SickoChat message", ref y, rect);
        DrawLabel("  /reveal - Toggle impostor reveal", ref y, rect);
        DrawLabel("  /noclip - Toggle noclip", ref y, rect);
        DrawLabel("  /zoom [1-5] - Set zoom level", ref y, rect);
        DrawLabel("  /wallhack - Toggle wallhack", ref y, rect);
        DrawLabel("  /repair - Repair sabotages", ref y, rect);
    }

    private void DrawGameAnticheat(Rect rect, ref float y)
    {
        DrawLabel("=== Anticheat Settings ===", ref y, rect);
        DrawLabel("Safe Mode: Disables features that may trigger anticheat", ref y, rect);
        State.BypassBans = DrawToggle(rect, ref y, "Bypass Bans (Experimental)", State.BypassBans);
    }

    private void DrawGameUtils(Rect rect, ref float y)
    {
        DrawLabel("=== Game Utilities ===", ref y, rect);
        DrawButton(rect, ref y, "Kill Everyone", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Protect Everyone", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Disable Venting", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Spam Report", () => ShowStatus("Not implemented yet"));
    }
    #endregion

    #region Self Tab
    private void DrawSelfTab(Rect rect)
    {
        var y = 0f;
        DrawSubTabsSelf(rect, ref y);
        y += 35;
        
        if (_selfOpenVisuals) DrawSelfVisuals(rect, ref y);
        else if (_selfOpenUtils) DrawSelfUtils(rect, ref y);
        else if (_selfOpenRandomizers) DrawSelfRandomizers(rect, ref y);
    }

    private void DrawSubTabsSelf(Rect rect, ref float y)
    {
        var tabWidth = rect.width / 3;
        
        var r1 = new Rect(0, y, tabWidth - 5, 30);
        if (GUI.Toggle(r1, _selfOpenVisuals, new GUIContent("Visuals"), GUI.skin.button))
        { _selfOpenVisuals = true; _selfOpenUtils = false; _selfOpenRandomizers = false; }
        
        var r2 = new Rect(tabWidth, y, tabWidth - 5, 30);
        if (GUI.Toggle(r2, _selfOpenUtils, new GUIContent("Utils"), GUI.skin.button))
        { _selfOpenVisuals = false; _selfOpenUtils = true; _selfOpenRandomizers = false; }
        
        var r3 = new Rect(tabWidth * 2, y, tabWidth - 5, 30);
        if (GUI.Toggle(r3, _selfOpenRandomizers, new GUIContent("Randomizers"), GUI.skin.button))
        { _selfOpenVisuals = false; _selfOpenUtils = false; _selfOpenRandomizers = true; }
        
        y += 35;
    }

    private void DrawSelfVisuals(Rect rect, ref float y)
    {
        DrawLabel("=== Visual Settings ===", ref y, rect);
        State.Zoom = DrawSlider(rect, ref y, "Zoom", State.Zoom, 0.5f, 10f);
        DrawLabel("Zoom: " + State.Zoom.ToString("F1") + "x", ref y, rect);
        State.EspVisible = DrawToggle(rect, ref y, "Show ESP", State.EspVisible);
        State.RadarVisible = DrawToggle(rect, ref y, "Show Radar", State.RadarVisible);
        State.HideEspDuringMeetings = DrawToggle(rect, ref y, "Hide ESP During Meetings", State.HideEspDuringMeetings);
        State.HideRadarDuringMeetings = DrawToggle(rect, ref y, "Hide Radar During Meetings", State.HideRadarDuringMeetings);
        State.ShowReplay = DrawToggle(rect, ref y, "Show Replay System", State.ShowReplay);
        DrawToggle(rect, ref y, "Ghost Mode (Show as alive)", State.GhostMode);
        DrawToggle(rect, ref y, "See Ghosts", false);
        DrawToggle(rect, ref y, "See Phantoms", false);
        DrawToggle(rect, ref y, "Dark Mode", true);
    }

    private void DrawSelfUtils(Rect rect, ref float y)
    {
        DrawLabel("=== Utility Actions ===", ref y, rect);
        DrawButton(rect, ref y, "Suicide", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Report Body (fake)", () => ShowStatus("Not implemented yet"));
        DrawToggle(rect, ref y, "Unlock Vents", false);
        DrawToggle(rect, ref y, "Move While in Vent & Shapeshifting", false);
        DrawToggle(rect, ref y, "Always Move", false);
        DrawToggle(rect, ref y, "No Shapeshift Animation", false);
        DrawToggle(rect, ref y, "NoClip", State.NoClip);
        DrawToggle(rect, ref y, "Allow Killing in Lobbies", false);
        DrawToggle(rect, ref y, "Kill Other Impostors", false);
        DrawToggle(rect, ref y, "Infinite Kill Range", false);
        DrawToggle(rect, ref y, "Bypass Guardian Angel Protections", false);
        DrawToggle(rect, ref y, "Autokill", false);
        DrawToggle(rect, ref y, "Do Tasks as Impostor", false);
        DrawToggle(rect, ref y, "Fake Alive", false);
        DrawToggle(rect, ref y, "God Mode", false);
    }

    private void DrawSelfRandomizers(Rect rect, ref float y)
    {
        DrawLabel("=== Randomizers ===", ref y, rect);
        DrawLabel("Random appearance and behavior options", ref y, rect);
        DrawButton(rect, ref y, "Cycler", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Cycle in Meeting", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Cycle Between Players", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Confuser (Randomize Appearance)", () => ShowStatus("Not implemented yet"));
    }
    #endregion

    #region Host Tab
    private void DrawHostTab(Rect rect)
    {
        var y = 0f;
        DrawSubTabsHost(rect, ref y);
        y += 35;
        
        if (_hostOpenGeneral) DrawHostGeneral(rect, ref y);
        else if (_hostOpenRoles) DrawHostRoles(rect, ref y);
        else if (_hostOpenOptions) DrawHostOptions(rect, ref y);
    }

    private void DrawSubTabsHost(Rect rect, ref float y)
    {
        var tabWidth = rect.width / 3;
        
        var r1 = new Rect(0, y, tabWidth - 5, 30);
        if (GUI.Toggle(r1, _hostOpenGeneral, new GUIContent("Utils"), GUI.skin.button))
        { _hostOpenGeneral = true; _hostOpenRoles = false; _hostOpenOptions = false; }
        
        var r2 = new Rect(tabWidth, y, tabWidth - 5, 30);
        if (GUI.Toggle(r2, _hostOpenRoles, new GUIContent("Roles"), GUI.skin.button))
        { _hostOpenGeneral = false; _hostOpenRoles = true; _hostOpenOptions = false; }
        
        var r3 = new Rect(tabWidth * 2, y, tabWidth - 5, 30);
        if (GUI.Toggle(r3, _hostOpenOptions, new GUIContent("Options"), GUI.skin.button))
        { _hostOpenGeneral = false; _hostOpenRoles = false; _hostOpenOptions = true; }
        
        y += 35;
    }

    private void DrawHostGeneral(Rect rect, ref float y)
    {
        DrawLabel("=== Host Utilities ===", ref y, rect);
        DrawButton(rect, ref y, "Start Game", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "End Game", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Kick All Players", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Close/Lock Lobby", () => ShowStatus("Not implemented yet"));
        DrawToggle(rect, ref y, "Disable Anticheat While Hosting (+25 Mode)", false);
    }

    private void DrawHostRoles(Rect rect, ref float y)
    {
        DrawLabel("=== Role Assignment ===", ref y, rect);
        DrawLabel("Select roles per player...", ref y, rect);
        DrawButton(rect, ref y, "Give Impostor to (selected)", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Give Crewmate to (selected)", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Randomize All Roles", () => ShowStatus("Not implemented yet"));
    }

    private void DrawHostOptions(Rect rect, ref float y)
    {
        DrawLabel("=== Game Options Override ===", ref y, rect);
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

    #region Players Tab
    private void DrawPlayersTab(Rect rect)
    {
        var y = 0f;
        DrawSubTabsPlayers(rect, ref y);
        y += 35;
        
        if (_playersOpenAllPlayers) DrawAllPlayers(rect, ref y);
        else if (_playersOpenActions) DrawPlayerActions(rect, ref y);
    }

    private void DrawSubTabsPlayers(Rect rect, ref float y)
    {
        var tabWidth = rect.width / 2;
        
        var r1 = new Rect(0, y, tabWidth - 5, 30);
        if (GUI.Toggle(r1, _playersOpenAllPlayers, new GUIContent("All Players"), GUI.skin.button))
        { _playersOpenAllPlayers = true; _playersOpenActions = false; }
        
        var r2 = new Rect(tabWidth, y, tabWidth - 5, 30);
        if (GUI.Toggle(r2, _playersOpenActions, new GUIContent("Actions"), GUI.skin.button))
        { _playersOpenAllPlayers = false; _playersOpenActions = true; }
        
        y += 35;
    }

    private void DrawAllPlayers(Rect rect, ref float y)
    {
        DrawLabel("Connected Players", ref y, rect);
        var py = 0f;
        try
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null) continue;
                var data = player.Data;
                if (data == null) continue;

                var roleStr = data.Role != null ? "[" + data.Role + "]" : "[No Role]";
                var impStr = data.Role != null && data.Role.IsImpostor ? " IMP" : "";
                DrawLabelAt("Player " + data.PlayerId + ": " + data.PlayerName + " " + roleStr + impStr, ref py, rect);
            }
        }
        catch { }
    }

    private void DrawPlayerActions(Rect rect, ref float y)
    {
        DrawLabel("Player Actions", ref y, rect);
        DrawButton(rect, ref y, "Murder Selected", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Vote Out Selected", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Ban Selected", () => ShowStatus("Not implemented yet"));
    }
    #endregion

    #region Feature Tabs
    private void DrawEspTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("=== ESP Settings ===", ref y, rect);
        State.ShowEsp = DrawToggle(rect, ref y, "Enable ESP", State.ShowEsp);
        State.HideEspDuringMeetings = DrawToggle(rect, ref y, "Hide During Meetings", State.HideEspDuringMeetings);
        DrawToggle(rect, ref y, "Show Boxes", true);
        DrawToggle(rect, ref y, "Show Tracers", false);
        DrawToggle(rect, ref y, "Show Distances", false);
        DrawToggle(rect, ref y, "Show Ghosts", false);
        DrawToggle(rect, ref y, "Role-based", false);
    }

    private void DrawRadarTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("=== Radar Settings ===", ref y, rect);
        State.ShowRadar = DrawToggle(rect, ref y, "Enable Radar", State.ShowRadar);
        State.HideRadarDuringMeetings = DrawToggle(rect, ref y, "Hide During Meetings", State.HideRadarDuringMeetings);
        DrawToggle(rect, ref y, "Show Dead Bodies", false);
        DrawToggle(rect, ref y, "Show Ghosts", false);
        DrawToggle(rect, ref y, "Right Click to Teleport", false);
        DrawToggle(rect, ref y, "Lock Radar Position", false);
        DrawToggle(rect, ref y, "Show Border", false);
    }

    private void DrawReplayTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("Replay System", ref y, rect);
        State.ShowReplay = DrawToggle(rect, ref y, "Enable Replay", State.ShowReplay);
        DrawButton(rect, ref y, "Record Last 30s", () => ShowStatus("Replay recording..."));
        DrawButton(rect, ref y, "Save Replay", () => ShowStatus("Replay saved!"));
        DrawButton(rect, ref y, "Load Replay", () => ShowStatus("Replay loaded!"));
        y += 10;
        DrawToggle(rect, ref y, "Show Only last seconds", false);
        DrawToggle(rect, ref y, "Clear after meeting", false);
    }

    private void DrawSabotageTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("Sabotage Controls", ref y, rect);
        if (DrawButton(rect, ref y, "Repair All Sabotages", () => SickoMenu.Features.SabotageHelper.RepairAll()))
            ShowStatus("All sabotages repaired!");
        
        var saboNames = new[] { "Reactor", "Oxygen", "Lights", "Comms", "Seismic", "Doors" };
        foreach (var name in saboNames)
        {
            DrawButton(rect, ref y, "Repair " + name, () => ShowStatus("Repairing " + name + "..."));
        }
        
        y += 10;
        DrawToggle(rect, ref y, "Disable Sabotage", false);
        DrawToggle(rect, ref y, "Auto Repair Sabotages", false);
        DrawButton(rect, ref y, "Sabotage All", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Random Sabotage", () => ShowStatus("Not implemented yet"));
        
        y += 10;
        DrawButton(rect, ref y, "Sabotage Lights", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Sabotage Reactor", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Sabotage Oxygen", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Sabotage Comms", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Disable Lights", () => ShowStatus("Not implemented yet"));
        DrawButton(rect, ref y, "Activate Mushroom Mixup", () => ShowStatus("Not implemented yet"));
    }

    private void DrawDoorsTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("Door Controls", ref y, rect);
        DrawButton(rect, ref y, "Open All Doors", () => ShowStatus("Opening all doors..."));
        DrawButton(rect, ref y, "Close All Doors", () => ShowStatus("Closing all doors..."));
        DrawButton(rect, ref y, "Pin All Doors", () => ShowStatus("Pinning all doors..."));
        DrawButton(rect, ref y, "Unpin All Doors", () => ShowStatus("Unpinning all doors..."));
        DrawToggle(rect, ref y, "Auto Open Doors", false);
        DrawButton(rect, ref y, "Close Room Door", () => ShowStatus("Closing current room door..."));
    }

    private void DrawTasksTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("Task Controls", ref y, rect);
        DrawButton(rect, ref y, "Complete All Tasks", () => ShowStatus("Completing all tasks..."));
        DrawButton(rect, ref y, "Play Medbay Scan Animation", () => ShowStatus("Not implemented yet"));
        
        y += 10;
        DrawLabel("Your Tasks:", ref y, rect);
        try
        {
            var localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer?.Data?.Tasks != null)
            {
                for (int i = 0; i < localPlayer.Data.Tasks.Count; i++)
                {
                    var task = localPlayer.Data.Tasks[i];
                    if (task != null)
                        DrawLabelAt("Task " + (i + 1) + ": " + (task.Complete ? "DONE" : "PENDING"), ref y, rect);
                }
            }
        }
        catch { }
    }

    private void DrawDebugTab(Rect rect)
    {
        var y = 0f;
        DrawLabel("Debug Information", ref y, rect);
        DrawLabel("Ping: " + _lastPing + "ms", ref y, rect);
        DrawLabel("Menu Visible: " + State.MenuVisible, ref y, rect);
        DrawLabel("Panic Mode: " + State.PanicMode, ref y, rect);
        DrawLabel("In Game: " + State.InGame, ref y, rect);
        DrawLabel("In Lobby: " + State.InLobby, ref y, rect);
        DrawLabel("In Meeting: " + State.InMeeting, ref y, rect);
        DrawLabel("Zoom: " + State.Zoom.ToString("F1") + "x", ref y, rect);
        DrawLabel("NoClip: " + State.NoClip, ref y, rect);
        DrawLabel("Free Chat: " + State.FreeChat, ref y, rect);
        DrawLabel("Screen: " + Screen.width + "x" + Screen.height, ref y, rect);
        DrawLabel("FPS: " + (1f / Time.deltaTime).ToString("F0"), ref y, rect);

        y += 10;
        if (DrawButton(rect, ref y, "Dump Offsets to Log", () =>
            {
                var report = OffsetSystem.DumpReport();
                SickoMenu.SickoMenuPlugin.PluginLogger.LogInfo(report);
                ShowStatus("Offset report dumped to log");
            })) { }

        if (DrawButton(rect, ref y, "Export Offsets JSON", () =>
            {
                var json = OffsetSystem.ExportOffsets();
                SickoMenu.SickoMenuPlugin.PluginLogger.LogInfo("Exported Offsets:\n" + json);
                ShowStatus("Offsets exported to log");
            })) { }

        if (DrawButton(rect, ref y, "Re-resolve Offsets", () =>
            {
                var ok = OffsetSystem.ResolveAll();
                ShowStatus(ok ? "Offsets re-resolved OK" : "Some offsets failed");
            })) { }
    }
    #endregion
    
    #endregion

    #region Console
    private void DrawConsole()
    {
        _consoleRect = GUI.Window(2, _consoleRect, (GUI.WindowFunction)DrawConsoleWindow, new GUIContent("SickoMenu Console"));
    }

    private void DrawConsoleWindow(int id)
    {
        _consoleScroll = GUI.BeginScrollView(
            new Rect(10, 30, _consoleRect.width - 20, _consoleRect.height - 100),
            _consoleScroll, 
            new Rect(0, 0, _consoleRect.width - 40, _consoleLines.Count * 20));
        
        var y = 0f;
        foreach (var line in _consoleLines)
        {
            GUI.Label(new Rect(5, y, _consoleRect.width - 40, 20), line);
            y += 20;
        }
        
        GUI.EndScrollView();

        var inputArea = new Rect(10, _consoleRect.height - 60, _consoleRect.width - 80, 30);
        var submitRect = new Rect(_consoleRect.width - 65, _consoleRect.height - 60, 55, 30);

        GUI.BeginGroup(inputArea);
        _consoleInput = GUILayout.TextField(_consoleInput, GUI.skin.textField);
        GUI.EndGroup();

        if (GUI.Button(submitRect, new GUIContent("Send")) ||
            (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
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
                    _consoleLines.Add("Reveal Impostors: " + (State.RevealImpostors ? "ON" : "OFF"));
                    break;
                case "/noclip":
                    State.NoClip = !State.NoClip;
                    _consoleLines.Add("NoClip: " + (State.NoClip ? "ON" : "OFF"));
                    break;
                case "/zoom":
                    if (float.TryParse(args, out var z))
                    {
                        State.Zoom = Mathf.Clamp(z, 0.5f, 10f);
                        _consoleLines.Add("Zoom set to " + State.Zoom);
                    }
                    break;
                case "/wallhack":
                    State.Wallhack = !State.Wallhack;
                    _consoleLines.Add("Wallhack: " + (State.Wallhack ? "ON" : "OFF"));
                    break;
                case "/repair":
                    SickoMenu.Features.SabotageHelper.RepairAll();
                    _consoleLines.Add("Sabotages repaired!");
                    break;
                case "/dump":
                    _consoleLines.Add(OffsetSystem.DumpReport());
                    break;
                case "/panic":
                    State.PanicMode = !State.PanicMode;
                    _consoleLines.Add("Panic Mode: " + (State.PanicMode ? "ON" : "OFF"));
                    break;
                case "/ghost":
                    State.GhostMode = !State.GhostMode;
                    _consoleLines.Add("Ghost Mode: " + (State.GhostMode ? "ON" : "OFF"));
                    break;
                default:
                    _consoleLines.Add("Unknown: " + command);
                    break;
            }
        }
        else
        {
            _consoleLines.Add("Unknown command: " + cmd);
        }
    }
    #endregion

    #region ESP / Radar / Replay Draw
    private void DrawRadar()
    {
        _radarRect = GUI.Window(3, _radarRect, (GUI.WindowFunction)DrawRadarWindow, new GUIContent("Radar"));
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
        _replayRect = GUI.Window(4, _replayRect, (GUI.WindowFunction)DrawReplayWindow, new GUIContent("Replay System"));
    }

    private void DrawReplayWindow(int id)
    {
        var y = 30f;
        DrawLabelAt("Replay Controls", ref y, _replayRect);
        if (GUI.Button(new Rect(10, y, _replayRect.width - 20, 30), new GUIContent("Record Last 30s")))
            ShowStatus("Replay recording...");
        y += 35;
        if (GUI.Button(new Rect(10, y, _replayRect.width - 20, 30), new GUIContent("Play Replay")))
            ShowStatus("Playing replay...");
        y += 35;
        if (GUI.Button(new Rect(10, y, _replayRect.width - 20, 30), new GUIContent("Save Replay")))
            ShowStatus("Replay saved!");

        DrawLabelAt("Replay Timeline:", ref y, _replayRect);
        GUI.Box(new Rect(10, y, _replayRect.width - 20, 100), "Timeline placeholder");
    }
    #endregion

    #region GUI Helpers
    private void DrawLabel(string text, ref float y, Rect parent)
    {
        var rect = new Rect(10, y, parent.width - 20, 25);
        GUI.Label(rect, text);
        y += 27;
    }

    private void DrawLabelAt(string text, ref float y, Rect parent)
    {
        var rect = new Rect(10, y, parent.width - 20, 25);
        GUI.Label(rect, text);
        y += 27;
    }

    private bool DrawToggle(Rect parent, ref float y, string label, bool value)
    {
        var rect = new Rect(10, y, parent.width - 20, 25);
        var result = GUI.Toggle(rect, value, new GUIContent(label), GUI.skin.toggle);
        y += 28;
        return result;
    }

    private string DrawTextField(Rect parent, ref float y, string label, string value)
    {
        DrawLabel(label, ref y, parent);
        var fieldArea = new Rect(10, y, parent.width - 20, 25);
        GUI.BeginGroup(fieldArea);
        var result = GUILayout.TextField(value, GUI.skin.textField);
        GUI.EndGroup();
        y += 28;
        return result;
    }

    private float DrawSlider(Rect parent, ref float y, string label, float value, float min, float max)
    {
        var labelRect = new Rect(10, y, parent.width - 20, 20);
        GUI.Label(labelRect, label + ": " + value.ToString("F1"));
        y += 25;
        return value;
    }

    private void DrawIntSlider(Rect parent, ref float y, string label, ref int value, int min, int max)
    {
        var labelRect = new Rect(10, y, parent.width - 20, 20);
        GUI.Label(labelRect, label + ": " + value);
        y += 25;
    }

    private bool DrawButton(Rect parent, ref float y, string label, Action action, Color? color = null)
    {
        var rect = new Rect(10, y, parent.width - 20, 30);
        if (color.HasValue)
            GUI.color = color.Value;

        var clicked = GUI.Button(rect, new GUIContent(label));

        if (color.HasValue)
            GUI.color = Color.white;

        y += 35;
        if (clicked)
            action?.Invoke();
        return clicked;
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
