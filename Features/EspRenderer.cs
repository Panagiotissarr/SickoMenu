using SickoMenu.Utils;

namespace SickoMenu.Features;

public static class EspRenderer
{
    private static readonly Dictionary<byte, EspPlayerInfo> _playerCache = [];
    private static float _lastCleanup;

    public class EspPlayerInfo
    {
        public byte PlayerId { get; set; }
        public string PlayerName { get; set; } = "";
        public UnityEngine.Vector3 WorldPosition { get; set; }
        public UnityEngine.Vector3 ScreenPosition { get; set; }
        public bool IsVisible { get; set; }
        public bool IsImpostor { get; set; }
        public bool IsDead { get; set; }
        public bool IsLocal { get; set; }
        public float Health { get; set; }
        public UnityEngine.Color Color { get; set; } = UnityEngine.Color.white;
        public float Distance { get; set; }
    }

    public static void Update()
    {
        if (State.PanicMode || !State.ShowEsp) return;

        try
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null) continue;

                var screenPos = UnityEngine.Camera.main != null
                    ? UnityEngine.Camera.main.WorldToScreenPoint(player.transform.position)
                    : UnityEngine.Vector3.zero;

                var info = new EspPlayerInfo
                {
                    PlayerId = player.PlayerId,
                    PlayerName = player.Data.PlayerName ?? $"Player {player.PlayerId}",
                    WorldPosition = player.transform.position,
                    ScreenPosition = screenPos,
                    IsVisible = screenPos.z > 0,
                    IsImpostor = player.Data.Role != null && player.Data.Role.IsImpostor,
                    IsDead = player.Data.IsDead,
                    IsLocal = player.AmOwner,
                    Health = player.Data.HealthPercent,
                    Distance = PlayerControl.LocalPlayer != null
                        ? UnityEngine.Vector2.Distance(
                            PlayerControl.LocalPlayer.transform.position,
                            player.transform.position)
                        : 0f
                };

                _playerCache[player.PlayerId] = info;
            }

            if (UnityEngine.Time.time - _lastCleanup > 5f)
            {
                _lastCleanup = UnityEngine.Time.time;
                var validIds = new System.Collections.Generic.HashSet<byte>();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player != null)
                        validIds.Add(player.PlayerId);
                }
                var toRemove = new List<byte>();
                foreach (var id in _playerCache.Keys)
                {
                    if (!validIds.Contains(id))
                        toRemove.Add(id);
                }
                foreach (var id in toRemove)
                    _playerCache.Remove(id);
            }
        }
        catch { }
    }

    public static IReadOnlyDictionary<byte, EspPlayerInfo> GetPlayers()
    {
        return _playerCache;
    }

    public static void Clear()
    {
        _playerCache.Clear();
    }
}
