namespace SickoMenu.Events;

public static class ProtectEventHandler
{
    public static void HandleProtect(byte protectorId, byte targetId, int colorId)
    {
        var data = new Dictionary<string, object>
        {
            ["ColorId"] = colorId
        };

        GameEventBus.Fire(GameEventType.Protect, protectorId, targetId, data);
        SickoMenuPlugin.PluginLogger.LogInfo(
            $"Protection: P{protectorId} -> P{targetId} (color: {colorId})");
    }

    public static void HandleProtectRemoved(byte playerId)
    {
        GameEventBus.Fire(GameEventType.Custom, playerId, data: new Dictionary<string, object>
        {
            ["Action"] = "ProtectionRemoved"
        });
        SickoMenuPlugin.PluginLogger.LogInfo($"Protection removed: P{playerId}");
    }
}
