namespace SickoMenu.RPC;

public static class RpcMurderPlayerHandler
{
    private static readonly Dictionary<byte, int> KillCounters = [];
    private static readonly Dictionary<byte, float> KillTimestamps = [];

    public static void HandleMurder(PlayerControl killer, PlayerControl target,
        MurderResultFlags resultFlags)
    {
        if (killer == null || target == null) return;

        var killerId = killer.PlayerId;
        var targetId = target.PlayerId;

        TrackKill(killerId, targetId);

        SickoMenuPlugin.PluginLogger.LogInfo(
            $"MurderPlayer: P{killerId} -> P{targetId} ({resultFlags})");
    }

    private static void TrackKill(byte killerId, byte targetId)
    {
        KillCounters.TryGetValue(killerId, out var count);
        KillCounters[killerId] = count + 1;
        KillTimestamps[killerId] = UnityEngine.Time.time;

        SickoMenuPlugin.PluginLogger.LogInfo(
            $"Kill tracked: P{killerId} now has {count + 1} kills");
    }

    public static int GetKillCount(byte playerId)
    {
        return KillCounters.GetValueOrDefault(playerId);
    }

    public static float GetTimeSinceLastKill(byte playerId)
    {
        if (KillTimestamps.TryGetValue(playerId, out var timestamp))
            return UnityEngine.Time.time - timestamp;
        return float.MaxValue;
    }

    public static void Reset()
    {
        KillCounters.Clear();
        KillTimestamps.Clear();
    }
}
