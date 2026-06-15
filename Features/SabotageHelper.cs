namespace SickoMenu.Features;

public static class SabotageHelper
{
    public static void RepairAll()
    {
        try
        {
            var shipStatus = ShipStatus.Instance;
            if (shipStatus == null) return;

            shipStatus.RpcUpdateSystem(
                SystemTypes.Sabotage,
                128
            );

            SickoMenuPlugin.PluginLogger.LogInfo("All sabotages repaired");
        }
        catch (System.Exception ex)
        {
            SickoMenuPlugin.PluginLogger.LogError($"Failed to repair sabotages: {ex.Message}");
        }
    }

    public static void RepairReactor()
    {
        RepairSystem(
            SystemTypes.Reactor,
            0
        );
    }

    public static void RepairOxygen()
    {
        RepairSystem(
            SystemTypes.LifeSupp,
            0
        );
    }

    public static void RepairLights()
    {
        RepairSystem(
            SystemTypes.Electrical,
            0
        );
    }

    public static void RepairComms()
    {
        RepairSystem(
            SystemTypes.Comms,
            0
        );
    }

    private static void RepairSystem(
        SystemTypes systemType,
        int amount)
    {
        try
        {
            var shipStatus = ShipStatus.Instance;
            if (shipStatus == null) return;

            shipStatus.RpcUpdateSystem(systemType, (byte)amount);
        }
        catch (System.Exception ex)
        {
            SickoMenuPlugin.PluginLogger.LogError(
                $"Failed to repair {systemType}: {ex.Message}");
        }
    }
}
