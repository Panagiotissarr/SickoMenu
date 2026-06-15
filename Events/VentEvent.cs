namespace SickoMenu.Events;

public static class VentEventHandler
{
    public static void HandleVentEnter(byte playerId, int ventId)
    {
        var data = new Dictionary<string, object>
        {
            ["VentId"] = ventId
        };

        GameEventBus.Fire(GameEventType.VentEnter, playerId, data: data);
        SickoMenuPlugin.PluginLogger.LogInfo($"P{playerId} entered vent {ventId}");
    }

    public static void HandleVentExit(byte playerId, int ventId)
    {
        var data = new Dictionary<string, object>
        {
            ["VentId"] = ventId
        };

        GameEventBus.Fire(GameEventType.VentExit, playerId, data: data);
        SickoMenuPlugin.PluginLogger.LogInfo($"P{playerId} exited vent {ventId}");
    }
}
