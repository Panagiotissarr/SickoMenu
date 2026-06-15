using SickoMenu.Utils;

namespace SickoMenu.Events;

public enum GameEventType
{
    Kill,
    Murder,
    MeetingStart,
    MeetingEnd,
    TaskComplete,
    VentEnter,
    VentExit,
    Shapeshift,
    ShapeshiftRevert,
    ReportBody,
    Disconnect,
    PlayerJoin,
    PlayerLeave,
    GameStart,
    GameEnd,
    SabotageStart,
    SabotageEnd,
    DoorOpen,
    DoorClose,
    ChatMessage,
    CheatDetected,
    Protect,
    ExileStart,
    ExileEnd,
    Custom
}

public class GameEvent : EventArgs
{
    public GameEventType Type { get; init; }
    public byte SourcePlayerId { get; init; }
    public byte? TargetPlayerId { get; init; }
    public Dictionary<string, object> Data { get; init; } = [];
    public float Timestamp { get; init; }
    public string CustomType { get; init; } = "";
}

public static class GameEventBus
{
    public static event EventHandler<GameEvent>? OnEvent;

    private static readonly List<GameEvent> EventHistory = [];
    private const int MaxHistory = 200;
    private static readonly object LockObj = new();

    public static void Fire(GameEvent evt)
    {
        lock (LockObj)
        {
            EventHistory.Add(evt);
            if (EventHistory.Count > MaxHistory)
                EventHistory.RemoveAt(0);
        }

        OnEvent?.Invoke(null, evt);

        if (evt.Type == GameEventType.CheatDetected)
            SickoMenuPlugin.PluginLogger.LogWarning(
                $"Cheat: {evt.Data.GetValueOrDefault("CheatType")} by P{evt.SourcePlayerId}");
    }

    public static void Fire(GameEventType type, byte sourceId, byte? targetId = null,
        Dictionary<string, object>? data = null)
    {
        Fire(new GameEvent
        {
            Type = type,
            SourcePlayerId = sourceId,
            TargetPlayerId = targetId,
            Data = data ?? [],
            Timestamp = UnityEngine.Time.time
        });
    }

    public static void FireCustom(string customType, byte sourceId,
        Dictionary<string, object>? data = null)
    {
        Fire(new GameEvent
        {
            Type = GameEventType.Custom,
            CustomType = customType,
            SourcePlayerId = sourceId,
            Data = data ?? [],
            Timestamp = UnityEngine.Time.time
        });
    }

    public static IReadOnlyList<GameEvent> GetHistory()
    {
        lock (LockObj)
            return EventHistory.ToList();
    }

    public static void Clear()
    {
        lock (LockObj)
            EventHistory.Clear();
    }

    public static string GetReport()
    {
        lock (LockObj)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== Event Bus Report ({EventHistory.Count} events) ===");
            foreach (var evt in EventHistory.TakeLast(30))
            {
                var typeStr = evt.Type == GameEventType.Custom
                    ? evt.CustomType
                    : evt.Type.ToString();
                sb.AppendLine(
                    $"[{evt.Timestamp:F2}] {typeStr}: P{evt.SourcePlayerId}" +
                    (evt.TargetPlayerId.HasValue ? $" -> P{evt.TargetPlayerId}" : ""));
            }
            return sb.ToString();
        }
    }
}
