namespace SickoMenu.Events;

public static class GameEndEventHandler
{
    public static void HandleGameEnd(GameOverReason endReason, bool showAd)
    {
        var data = new Dictionary<string, object>
        {
            ["EndReason"] = endReason.ToString(),
            ["ShowAd"] = showAd
        };

        GameEventBus.Fire(GameEventType.GameEnd, 0, data: data);
        SickoMenuPlugin.PluginLogger.LogInfo(
            $"Game ended: {endReason} (showAd: {showAd})");
    }

    public static void HandleGameStart()
    {
        GameEventBus.Fire(GameEventType.GameStart, 0);
        SickoMenuPlugin.PluginLogger.LogInfo("Game started");
    }
}
