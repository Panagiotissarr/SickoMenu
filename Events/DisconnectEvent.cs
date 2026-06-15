namespace SickoMenu.Events;

public static class DisconnectEventHandler
{
    public static void HandleDisconnect(byte playerId, DisconnectReasons reason, string? message = null)
    {
        var data = new Dictionary<string, object>
        {
            ["Reason"] = reason.ToString()
        };

        if (!string.IsNullOrEmpty(message))
            data["Message"] = message;

        GameEventBus.Fire(GameEventType.Disconnect, playerId, data: data);

        if (reason == DisconnectReasons.Banned)
            SickoMenuPlugin.PluginLogger.LogWarning(
                $"Player P{playerId} was banned: {message}");
        else
            SickoMenuPlugin.PluginLogger.LogInfo(
                $"Player P{playerId} disconnected: {reason}");
    }

    public static void HandlePlayerJoin(byte playerId, string playerName)
    {
        var data = new Dictionary<string, object>
        {
            ["PlayerName"] = playerName
        };

        GameEventBus.Fire(GameEventType.PlayerJoin, playerId, data: data);
        SickoMenuPlugin.PluginLogger.LogInfo(
            $"Player joined: P{playerId} ({playerName})");
    }

    public static void HandlePlayerLeave(byte playerId)
    {
        GameEventBus.Fire(GameEventType.PlayerLeave, playerId);
        SickoMenuPlugin.PluginLogger.LogInfo($"Player left: P{playerId}");
    }
}
