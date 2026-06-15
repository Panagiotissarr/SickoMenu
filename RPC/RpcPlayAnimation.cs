namespace SickoMenu.RPC;

public enum AnimationType : byte
{
    None = 0,
    EnterVent = 1,
    ExitVent = 2,
    Shapeshift = 3,
    RevertShapeshift = 4,
    Pet = 5,
    Scan = 6,
    Custom = 100
}

public static class RpcPlayAnimationHandler
{
    private static readonly Dictionary<byte, AnimationType> LastAnimations = new Dictionary<byte, AnimationType>();
    private static readonly Dictionary<byte, float> AnimationTimestamps = new Dictionary<byte, float>();

    public static void Handle(PlayerControl player, byte animType)
    {
        if (player == null) return;

        var animation = (AnimationType)animType;
        LastAnimations[player.PlayerId] = animation;
        AnimationTimestamps[player.PlayerId] = UnityEngine.Time.time;

        SickoMenuPlugin.PluginLogger.LogInfo(
            $"Animation: P{player.PlayerId} -> {animation}");

        switch (animation)
        {
            case AnimationType.EnterVent:
                Events.VentEventHandler.HandleVentEnter(player.PlayerId, 0);
                break;
            case AnimationType.ExitVent:
                Events.VentEventHandler.HandleVentExit(player.PlayerId, 0);
                break;
            case AnimationType.Shapeshift:
                Events.ShapeshiftEventHandler.HandleShapeshift(player.PlayerId, null);
                break;
            case AnimationType.RevertShapeshift:
                Events.ShapeshiftEventHandler.HandleShapeshiftRevert(player.PlayerId);
                break;
        }
    }

    public static AnimationType? GetLastAnimation(byte playerId)
    {
        return LastAnimations.GetValueOrDefault(playerId);
    }

    public static float GetTimeSinceAnimation(byte playerId)
    {
        if (AnimationTimestamps.TryGetValue(playerId, out var timestamp))
            return UnityEngine.Time.time - timestamp;
        return float.MaxValue;
    }

    public static void SendPlayAnimation(byte animType)
    {
        RpcHandler.SendRpc(100, writer =>
        {
            writer.Write(animType);
        });
    }

    public static void Reset()
    {
        LastAnimations.Clear();
        AnimationTimestamps.Clear();
    }
}
