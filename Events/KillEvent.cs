namespace SickoMenu.Events;

public static class KillEventHandler
{
    public static void HandleKill(byte killerId, byte victimId, MurderResultFlags resultFlags)
    {
        var data = new Dictionary<string, object>
        {
            ["VictimId"] = victimId,
            ["ResultFlags"] = (int)resultFlags
        };

        GameEventBus.Fire(GameEventType.Kill, killerId, victimId, data);

        SickoMenuPlugin.PluginLogger.LogInfo(
            $"Kill: P{killerId} -> P{victimId} ({resultFlags})");
    }

    public static void HandleMurder(byte killerId, byte victimId)
    {
        var data = new Dictionary<string, object>
        {
            ["VictimId"] = victimId,
            ["IsSuccessful"] = true
        };

        GameEventBus.Fire(GameEventType.Murder, killerId, victimId, data);
    }
}
