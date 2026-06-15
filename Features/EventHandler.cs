using SickoMenu.Utils;

namespace SickoMenu.Features;

public class GameEventArgs
{
    public string EventType { get; init; } = "";
    public float Timestamp { get; init; }
    public byte PlayerId { get; init; }
    public Dictionary<string, object> Data { get; init; } = new Dictionary<string, object>();
}

public static class EventHandler
{
    private static readonly List<GameEventArgs> _eventHistory = new List<GameEventArgs>();
    private static readonly int MaxEvents = 500;

    public static event Action<GameEventArgs>? OnGameEvent;

    public static IReadOnlyList<GameEventArgs> EventHistory => _eventHistory.AsReadOnly();

    public static void FireEvent(string type, byte playerId, Dictionary<string, object>? data = null)
    {
        var evt = new GameEventArgs
        {
            EventType = type,
            Timestamp = UnityEngine.Time.time,
            PlayerId = playerId,
            Data = data ?? new Dictionary<string, object>()
        };

        if (_eventHistory.Count >= MaxEvents)
            _eventHistory.RemoveAt(0);

        _eventHistory.Add(evt);
        OnGameEvent?.Invoke(evt);
    }

    public static void RecordKill(byte killerId, byte victimId)
    {
        FireEvent("Kill", killerId, new Dictionary<string, object>
        {
            ["VictimId"] = victimId
        });
        ReplaySystem.RecordEvent("Kill", killerId, victimId);
    }

    public static void RecordMeeting(byte reporterId, byte? bodyId)
    {
        FireEvent("Meeting", reporterId, new Dictionary<string, object>
        {
            ["BodyId"] = bodyId ?? -1
        });
        ReplaySystem.RecordEvent("Meeting", reporterId, bodyId);
    }

    public static void RecordTaskComplete(byte playerId, uint taskIdx)
    {
        FireEvent("TaskComplete", playerId, new Dictionary<string, object>
        {
            ["TaskIndex"] = (int)taskIdx
        });
    }

    public static void RecordVent(byte playerId, bool entering)
    {
        FireEvent(entering ? "VentEnter" : "VentExit", playerId);
        ReplaySystem.RecordEvent(entering ? "VentEnter" : "VentExit", playerId);
    }

    public static void RecordShapeshift(byte playerId, byte? targetId)
    {
        FireEvent("Shapeshift", playerId, new Dictionary<string, object>
        {
            ["TargetId"] = targetId ?? -1
        });
    }

    public static void RecordDisconnect(byte playerId, DisconnectReasons reason)
    {
        FireEvent("Disconnect", playerId, new Dictionary<string, object>
        {
            ["Reason"] = reason.ToString()
        });
    }

    public static void RecordCheatDetected(string cheatType, byte playerId)
    {
        FireEvent("CheatDetected", playerId, new Dictionary<string, object>
        {
            ["CheatType"] = cheatType
        });
        SickoMenuPlugin.PluginLogger.LogWarning(
            $"Cheat detected: {cheatType} by Player {playerId}");
    }

    public static void ClearHistory()
    {
        _eventHistory.Clear();
    }

    public static string GetHistoryReport()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Event History ===");
        sb.AppendLine($"Total events: {_eventHistory.Count}");
        sb.AppendLine();

        foreach (var evt in _eventHistory.TakeLast(50))
        {
            sb.AppendLine(
                $"[{evt.Timestamp:F2}] {evt.EventType} by Player {evt.PlayerId}");
            foreach (var (key, val) in evt.Data)
            {
                sb.AppendLine($"  {key}: {val}");
            }
        }

        return sb.ToString();
    }
}
