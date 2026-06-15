namespace SickoMenu.Events;

public static class ShapeshiftEventHandler
{
    public static void HandleShapeshift(byte playerId, byte? targetId)
    {
        var data = new Dictionary<string, object>();
        if (targetId.HasValue)
            data["TargetId"] = targetId.Value;

        GameEventBus.Fire(GameEventType.Shapeshift, playerId, targetId, data);
        SickoMenuPlugin.PluginLogger.LogInfo(
            $"Shapeshift: P{playerId} -> {(targetId.HasValue ? $"P{targetId}" : "revert")}");
    }

    public static void HandleShapeshiftRevert(byte playerId)
    {
        GameEventBus.Fire(GameEventType.ShapeshiftRevert, playerId);
        SickoMenuPlugin.PluginLogger.LogInfo($"Shapeshift reverted: P{playerId}");
    }
}
