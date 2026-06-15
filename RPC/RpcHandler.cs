using Hazel;
using SickoMenu.Utils;

namespace SickoMenu.RPC;

public enum CustomRpcCalls : byte
{
    SetRole = 45,
    SetLevel = 46,
    SetCosmetics = 47,
    KickPlayer = 48,
    BanPlayer = 49,
    StartMeeting = 50,
    SendChat = 51,
    CompleteTask = 52,
    KillPlayer = 53,
    Shapeshift = 54,
    ProtectPlayer = 55,
    ReportBody = 56,
    SyncSettings = 57,
    SetName = 58,
    SetColor = 59,
    SetHat = 60,
    SetPet = 61,
    SetSkin = 62,
    SetVisor = 63,
    SetNameplate = 64,
    SetPlatform = 65,
    SetFriendCode = 66,
    Custom = 100
}

public static class RpcHandler
{
    private static readonly Dictionary<byte, Action<MessageReader>> Handlers = new Dictionary<byte, Action<MessageReader>>();
    private static readonly object LockObj = new();

    static RpcHandler()
    {
        RegisterDefaultHandlers();
    }

    private static void RegisterDefaultHandlers()
    {
        Register((byte)CustomRpcCalls.SetRole, HandleSetRole);
        Register((byte)CustomRpcCalls.SetLevel, HandleSetLevel);
        Register((byte)CustomRpcCalls.SetCosmetics, HandleSetCosmetics);
        Register((byte)CustomRpcCalls.KickPlayer, HandleKickPlayer);
        Register((byte)CustomRpcCalls.BanPlayer, HandleBanPlayer);
        Register((byte)CustomRpcCalls.Custom, HandleCustomRpc);
    }

    public static void Register(byte callId, Action<MessageReader> handler)
    {
        lock (LockObj)
        {
            Handlers[callId] = handler;
        }
    }

    public static void Unregister(byte callId)
    {
        lock (LockObj)
        {
            Handlers.Remove(callId);
        }
    }

    public static bool Handle(byte callId, MessageReader reader)
    {
        lock (LockObj)
        {
            if (Handlers.TryGetValue(callId, out var handler))
            {
                try
                {
                    handler(reader);
                    return true;
                }
                catch (Exception ex)
                {
                    SickoMenuPlugin.PluginLogger.LogError(
                        $"RPC handler error for {callId}: {ex.Message}");
                    return false;
                }
            }
        }
        return false;
    }

    public static void SendRpc(byte callId, Action<MessageWriter> writeAction,
        bool sendToServer = true)
    {
        try
        {
            var localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer == null) return;

            var writer = MessageWriter.Get(SendOption.Reliable);
            writer.StartMessage(5);
            writer.Write(localPlayer.NetId);
            writer.Write(callId);
            writeAction(writer);
            writer.EndMessage();

            if (sendToServer)
            {
            }
            else
            {
                writer.Recycle();
                return;
            }

            writer.Recycle();
        }
        catch (Exception ex)
        {
            SickoMenuPlugin.PluginLogger.LogError($"Failed to send RPC {callId}: {ex.Message}");
        }
    }

    public static void SendChat(string message)
    {
        SendRpc((byte)CustomRpcCalls.SendChat, writer =>
        {
            writer.Write(message);
        });
    }

    public static void SendKill(byte targetPlayerId)
    {
        SendRpc((byte)CustomRpcCalls.KillPlayer, writer =>
        {
            writer.Write(targetPlayerId);
        });
    }

    public static void SendCompleteTask(uint taskIdx)
    {
        SendRpc((byte)CustomRpcCalls.CompleteTask, writer =>
        {
            writer.Write(taskIdx);
        });
    }

    public static void SendShapeshift(byte targetPlayerId, bool animate)
    {
        SendRpc((byte)CustomRpcCalls.Shapeshift, writer =>
        {
            writer.Write(targetPlayerId);
            writer.Write(animate);
        });
    }

    public static void SendReportBody(byte bodyPlayerId)
    {
        SendRpc((byte)CustomRpcCalls.ReportBody, writer =>
        {
            writer.Write(bodyPlayerId);
        });
    }

    public static void SendStartMeeting(byte? bodyPlayerId)
    {
        SendRpc((byte)CustomRpcCalls.StartMeeting, writer =>
        {
            writer.Write(bodyPlayerId ?? 255);
        });
    }

    public static void SendSyncSettings(byte[] settings)
    {
        SendRpc((byte)CustomRpcCalls.SyncSettings, writer =>
        {
            writer.WriteBytesAndSize(settings);
        });
    }

    public static void SendProtect(byte targetPlayerId, int colorId)
    {
        SendRpc((byte)CustomRpcCalls.ProtectPlayer, writer =>
        {
            writer.Write(targetPlayerId);
            writer.WritePacked(colorId);
        });
    }

    private static void HandleSetRole(MessageReader reader) { }
    private static void HandleSetLevel(MessageReader reader) { }
    private static void HandleSetCosmetics(MessageReader reader) { }
    private static void HandleKickPlayer(MessageReader reader) { }
    private static void HandleBanPlayer(MessageReader reader) { }
    private static void HandleCustomRpc(MessageReader reader) { }
}
