namespace SickoMenu.RPC;

public static class RpcCloseDoorsOfTypeHandler
{
    private static readonly Dictionary<SystemTypes, float> DoorCloseTimestamps = new Dictionary<SystemTypes, float>();
    private const float DoorCooldown = 5f;

    public static void Handle(SystemTypes doorType)
    {
        if (!CanCloseDoors(doorType))
        {
            SickoMenuPlugin.PluginLogger.LogWarning(
                $"Door close on cooldown: {doorType}");
            return;
        }

        DoorCloseTimestamps[doorType] = UnityEngine.Time.time;
        SickoMenuPlugin.PluginLogger.LogInfo($"Doors closed: {doorType}");
    }

    public static void HandleOpen(SystemTypes doorType)
    {
        DoorCloseTimestamps.Remove(doorType);
        SickoMenuPlugin.PluginLogger.LogInfo($"Doors opened: {doorType}");
    }

    public static bool CanCloseDoors(SystemTypes doorType)
    {
        if (DoorCloseTimestamps.TryGetValue(doorType, out var lastClose))
            return UnityEngine.Time.time - lastClose >= DoorCooldown;
        return true;
    }

    public static float GetCooldownRemaining(SystemTypes doorType)
    {
        if (DoorCloseTimestamps.TryGetValue(doorType, out var lastClose))
        {
            var remaining = DoorCooldown - (UnityEngine.Time.time - lastClose);
            return Math.Max(0, remaining);
        }
        return 0f;
    }

    public static void Reset()
    {
        DoorCloseTimestamps.Clear();
    }
}

public static class RpcCloseDoorsOfTypeSender
{
    public static void CloseDoorsOfType(SystemTypes type)
    {
        try
        {
            var shipStatus = ShipStatus.Instance;
            if (shipStatus == null) return;

            shipStatus.RpcCloseDoorsOfType(type);
            RpcCloseDoorsOfTypeHandler.Handle(type);
        }
        catch (Exception ex)
        {
            SickoMenuPlugin.PluginLogger.LogError(
                $"Failed to close doors {type}: {ex.Message}");
        }
    }

    public static void OpenAllDoors()
    {
        try
        {
            var shipStatus = ShipStatus.Instance;
            if (shipStatus == null) return;

            var doorTypes = new[]
            {
                SystemTypes.Admin, SystemTypes.Cafeteria, SystemTypes.Electrical,
                SystemTypes.MedBay, SystemTypes.Nav, SystemTypes.Reactor,
                SystemTypes.Security, SystemTypes.Shields, SystemTypes.Storage,
                SystemTypes.UpperEngine, SystemTypes.Weapons
            };

            foreach (var type in doorTypes)
            {
                shipStatus.RpcCloseDoorsOfType(type);
                RpcCloseDoorsOfTypeHandler.HandleOpen(type);
            }

            SickoMenuPlugin.PluginLogger.LogInfo("All doors opened");
        }
        catch (Exception ex)
        {
            SickoMenuPlugin.PluginLogger.LogError(
                $"Failed to open all doors: {ex.Message}");
        }
    }
}
