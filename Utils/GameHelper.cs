using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace SickoMenu.Utils;

public static class GameHelper
{
    private static IntPtr _gameAssembly;

    public static bool IsInGame()
    {
        try
        {
            var amogus = AmongUsClient_Instance();
            if (amogus == IntPtr.Zero) return false;
            var state = GetGameState(amogus);
            return state >= 3 && state <= 6;
        }
        catch { return false; }
    }

    public static bool IsInLobby()
    {
        try
        {
            var amogus = AmongUsClient_Instance();
            if (amogus == IntPtr.Zero) return false;
            return GetGameState(amogus) == 2;
        }
        catch { return false; }
    }

    public static bool IsInMeeting()
    {
        try
        {
            var meetingHud = MeetingHud_Instance();
            return meetingHud != IntPtr.Zero;
        }
        catch { return false; }
    }

    public static IntPtr GetLocalPlayer()
    {
        try
        {
            var playerCtrlClass = IL2CPP.GetClass("Assembly-CSharp", "AmongUs.GameOptions", "PlayerControl");
            if (playerCtrlClass == IntPtr.Zero) return IntPtr.Zero;
            var localPlayer = IL2CPP.Invoke(IL2CPP.GetClass("Assembly-CSharp", "", "PlayerControl"), "get_LocalPlayer", IntPtr.Zero);
            return localPlayer;
        }
        catch { return IntPtr.Zero; }
    }

    private static IntPtr AmongUsClient_Instance()
    {
        try
        {
            var klass = IL2CPP.GetClass("Assembly-CSharp", "", "AmongUsClient");
            if (klass == IntPtr.Zero) return IntPtr.Zero;
            var prop = IL2CPP.GetProperty(klass, "Instance");
            if (prop == IntPtr.Zero) return IntPtr.Zero;
            return IL2CPP.Invoke(klass, "Instance", IntPtr.Zero);
        }
        catch { return IntPtr.Zero; }
    }

    private static IntPtr MeetingHud_Instance()
    {
        try
        {
            var klass = IL2CPP.GetClass("Assembly-CSharp", "", "MeetingHud");
            if (klass == IntPtr.Zero) return IntPtr.Zero;
            return IL2CPP.Invoke(klass, "Instance", IntPtr.Zero);
        }
        catch { return IntPtr.Zero; }
    }

    private static int GetGameState(IntPtr client)
    {
        var klass = IL2CPP.GetClass("Assembly-CSharp", "", "AmongUsClient");
        if (klass == IntPtr.Zero) return 0;
        var prop = IL2CPP.GetProperty(klass, "GameState");
        if (prop == IntPtr.Zero) return 0;
        var state = IL2CPP.Invoke(client, "get_GameState", IntPtr.Zero);
        return state != IntPtr.Zero ? System.Runtime.InteropServices.Marshal.ReadInt32(state) : 0;
    }

    public static Il2CppObjectBase? GetPlayerName(IntPtr playerControl)
    {
        try
        {
            return null;
        }
        catch { return null; }
    }

    public static void SendChat(string message)
    {
        try
        {
            var playerControl = GetLocalPlayer();
            if (playerControl == IntPtr.Zero) return;

            var chatController = IL2CPP.Invoke(
                IL2CPP.GetClass("Assembly-CSharp", "", "ChatController"),
                "Instance",
                IntPtr.Zero);
            if (chatController == IntPtr.Zero) return;

            var str = IL2CPP.ManagedStringToIl2Cpp(message);
            IL2CPP.Invoke(chatController, "AddChat", playerControl, str, false);
        }
        catch { }
    }

    public static void RepairSabotage()
    {
        try
        {
            var shipStatus = IL2CPP.Invoke(
                IL2CPP.GetClass("Assembly-CSharp", "", "ShipStatus"),
                "Instance",
                IntPtr.Zero);
            if (shipStatus == IntPtr.Zero) return;

            IL2CPP.Invoke(shipStatus, "RpcUpdateSystem",
                (int)SystemTypes.Sabotage, 128);
        }
        catch { }
    }

    public static void SetKillCooldown(float cooldown)
    {
        try
        {
            var playerControl = GetLocalPlayer();
            if (playerControl == IntPtr.Zero) return;

            var data = IL2CPP.Invoke(playerControl, "get_Data", IntPtr.Zero);
            if (data == IntPtr.Zero) return;

            if (data == IntPtr.Zero) return;

            var roleManager = IL2CPP.Invoke(
                IL2CPP.GetClass("Assembly-CSharp", "", "RoleManager"),
                "Instance",
                IntPtr.Zero);
            if (roleManager == IntPtr.Zero) return;

            var allRoles = IL2CPP.Invoke(roleManager, "get_AllRoles", IntPtr.Zero);
            if (allRoles == IntPtr.Zero) return;
        }
        catch { }
    }
}

public enum SystemTypes : uint
{
    Hallway = 0,
    Storage = 1,
    Cafeteria = 2,
    Reactor = 3,
    UpperEngine = 4,
    Nav = 5,
    Admin = 6,
    Electrical = 7,
    LifeSupp = 8,
    Shields = 9,
    Comms = 10,
    Weapons = 11,
    MedBay = 12,
    O2 = 13,
    O2N = 14,
    LowerEngine = 15,
    Security = 16,
    Simulator = 17,
    Launchpad = 18,
    Laboratory = 19,
    Office = 20,
    Greenhouse = 21,
    Balcony = 22,
    Toilet = 23,
    Engine = 24,
    Outside = 25,
    Sabotage = 128,
    Decontamination = 254,
    ExitOnly = 255,
}
