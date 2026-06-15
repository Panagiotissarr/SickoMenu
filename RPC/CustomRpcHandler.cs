namespace SickoMenu.RPC;

public static class CustomRpcHandler
{
    private static readonly Dictionary<byte, Func<MessageReader, bool>> CustomHandlers = [];
    private static readonly object LockObj = new();

    static CustomRpcHandler()
    {
        RegisterDefaultCustomHandlers();
    }

    private static void RegisterDefaultCustomHandlers()
    {
        Register(0, HandleSickoChat);
        Register(1, HandleCustomAnimation);
        Register(2, HandleForceRole);
        Register(3, HandleForceStart);
        Register(4, HandleForceEnd);
    }

    public static void Register(byte subCmd, Func<MessageReader, bool> handler)
    {
        lock (LockObj)
        {
            CustomHandlers[subCmd] = handler;
        }
    }

    public static bool Handle(byte subCmd, MessageReader reader)
    {
        lock (LockObj)
        {
            if (CustomHandlers.TryGetValue(subCmd, out var handler))
            {
                try
                {
                    return handler(reader);
                }
                catch (Exception ex)
                {
                    SickoMenuPlugin.PluginLogger.LogError(
                        $"Custom RPC handler error for {subCmd}: {ex.Message}");
                    return false;
                }
            }
        }
        return false;
    }

    private static bool HandleSickoChat(MessageReader reader)
    {
        try
        {
            var message = reader.ReadString();
            SickoMenuPlugin.PluginLogger.LogInfo($"SickoChat: {message}");
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool HandleCustomAnimation(MessageReader reader)
    {
        try
        {
            var animType = reader.ReadByte();
            SickoMenuPlugin.PluginLogger.LogInfo($"Custom animation: {animType}");
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool HandleForceRole(MessageReader reader)
    {
        try
        {
            var targetId = reader.ReadByte();
            var roleType = reader.ReadByte();
            SickoMenuPlugin.PluginLogger.LogInfo(
                $"Force role: P{targetId} -> {(RoleTypes)roleType}");
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool HandleForceStart(MessageReader reader)
    {
        SickoMenuPlugin.PluginLogger.LogInfo("Force start game");
        return true;
    }

    private static bool HandleForceEnd(MessageReader reader)
    {
        SickoMenuPlugin.PluginLogger.LogInfo("Force end game");
        return true;
    }

    public static void SendCustomRpc(byte subCmd, Action<MessageWriter> writeAction)
    {
        RpcHandler.SendRpc(100, writer =>
        {
            writer.Write(subCmd);
            writeAction(writer);
        });
    }

    public static void SendSickoChatMessage(string message)
    {
        SendCustomRpc(0, writer =>
        {
            writer.Write(message);
        });
    }

    public static void SendForceRole(byte targetId, RoleTypes role)
    {
        SendCustomRpc(2, writer =>
        {
            writer.Write(targetId);
            writer.Write((byte)role);
        });
    }

    public static void SendForceStart()
    {
        SendCustomRpc(3, _ => { });
    }

    public static void SendForceEnd()
    {
        SendCustomRpc(4, _ => { });
    }
}
