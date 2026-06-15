using SickoMenu.Utils;

namespace SickoMenu.Features;

public class ReplayFrame
{
    public float Timestamp { get; set; }
    public Dictionary<byte, ReplayPlayerState> PlayerStates { get; set; } = new Dictionary<byte, ReplayPlayerState>();
    public List<ReplayEvent> Events { get; set; } = new List<ReplayEvent>();
}

public class ReplayPlayerState
{
    public byte PlayerId { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public bool IsDead { get; set; }
    public bool IsDisconnected { get; set; }
}

public class ReplayEvent
{
    public float Timestamp { get; set; }
    public string Type { get; set; } = "";
    public byte PlayerId { get; set; }
    public byte? TargetId { get; set; }
    public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
}

public static class ReplaySystem
{
    private static readonly List<ReplayFrame> _recordedFrames = new List<ReplayFrame>();
    private static readonly List<ReplayEvent> _recordedEvents = new List<ReplayEvent>();
    private static bool _isRecording;
    private static bool _isPlaying;
    private static int _playbackIndex;
    private static float _recordStartTime;
    private static float _playbackStartTime;
    private const int MaxFrames = 900;

    private static readonly System.Collections.Concurrent.ConcurrentQueue<ReplayFrame> _frameQueue = new();
    private static readonly System.Threading.CancellationTokenSource _cts = new();

    public static bool IsRecording => _isRecording;
    public static bool IsPlaying => _isPlaying;
    public static int RecordedFrameCount => _recordedFrames.Count;
    public static int RecordedEventCount => _recordedEvents.Count;

    public static void StartRecording()
    {
        if (_isRecording) return;
        _isRecording = true;
        _recordStartTime = UnityEngine.Time.time;
        _recordedFrames.Clear();
        _recordedEvents.Clear();
        SickoMenuPlugin.PluginLogger.LogInfo("Replay recording started");
    }

    public static void StopRecording()
    {
        _isRecording = false;
        SickoMenuPlugin.PluginLogger.LogInfo(
            $"Replay recording stopped. Captured {_recordedFrames.Count} frames, {_recordedEvents.Count} events");
    }

    public static void RecordFrame()
    {
        if (!_isRecording) return;
        if (_recordedFrames.Count >= MaxFrames)
        {
            _recordedFrames.RemoveAt(0);
        }

        var frame = new ReplayFrame
        {
            Timestamp = UnityEngine.Time.time - _recordStartTime
        };

        try
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null) continue;

                var state = new ReplayPlayerState
                {
                    PlayerId = player.PlayerId,
                    X = player.transform.position.x,
                    Y = player.transform.position.y,
                    IsDead = player.Data.IsDead,
                    IsDisconnected = player.Data.Disconnected
                };
                frame.PlayerStates[player.PlayerId] = state;
            }
        }
        catch { }

        _recordedFrames.Add(frame);
    }

    public static void RecordEvent(string type, byte playerId, byte? targetId = null,
        Dictionary<string, string>? data = null)
    {
        if (!_isRecording) return;

        _recordedEvents.Add(new ReplayEvent
        {
            Timestamp = UnityEngine.Time.time - _recordStartTime,
            Type = type,
            PlayerId = playerId,
            TargetId = targetId,
            Data = data ?? new Dictionary<string, string>()
        });
    }

    public static void StartPlayback()
    {
        if (_isPlaying) return;
        if (_recordedFrames.Count == 0)
        {
            SickoMenuPlugin.PluginLogger.LogWarning("No replay data to play");
            return;
        }

        _isPlaying = true;
        _playbackIndex = 0;
        _playbackStartTime = UnityEngine.Time.time;
        SickoMenuPlugin.PluginLogger.LogInfo(
            $"Replay playback started: {_recordedFrames.Count} frames");
    }

    public static void StopPlayback()
    {
        _isPlaying = false;
        SickoMenuPlugin.PluginLogger.LogInfo("Replay playback stopped");
    }

    public static void UpdatePlayback()
    {
        if (!_isPlaying) return;

        var elapsed = UnityEngine.Time.time - _playbackStartTime;

        while (_playbackIndex < _recordedFrames.Count &&
               _recordedFrames[_playbackIndex].Timestamp <= elapsed)
        {
            var frame = _recordedFrames[_playbackIndex];

            foreach (var (playerId, state) in frame.PlayerStates)
            {
                try
                {
                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (player != null && player.PlayerId == playerId)
                        {
                            var pos = player.transform.position;
                            pos.x = state.X;
                            pos.y = state.Y;
                            player.transform.position = pos;
                        }
                    }
                }
                catch { }
            }

            _playbackIndex++;
        }

        if (_playbackIndex >= _recordedFrames.Count)
        {
            StopPlayback();
            SickoMenuPlugin.PluginLogger.LogInfo("Replay playback finished");
        }
    }

    public static void Clear()
    {
        _recordedFrames.Clear();
        _recordedEvents.Clear();
        _isRecording = false;
        _isPlaying = false;
        SickoMenuPlugin.PluginLogger.LogInfo("Replay data cleared");
    }

    public static string GetStats()
    {
        return $"Frames: {_recordedFrames.Count}, Events: {_recordedEvents.Count}, " +
               $"Duration: {(_isRecording ? UnityEngine.Time.time - _recordStartTime : 0):F1}s";
    }

    public static string Export()
    {
        var data = new System.Text.StringBuilder();
        data.AppendLine("=== SickoMenu Replay Export ===");
        data.AppendLine($"Frames: {_recordedFrames.Count}");
        data.AppendLine($"Events: {_recordedEvents.Count}");
        data.AppendLine();

        foreach (var evt in _recordedEvents)
        {
            data.AppendLine($"[{evt.Timestamp:F2}] {evt.Type} Player={evt.PlayerId}" +
                            (evt.TargetId.HasValue ? $" Target={evt.TargetId}" : ""));
        }

        return data.ToString();
    }
}
