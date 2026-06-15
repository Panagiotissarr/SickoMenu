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
                UnityEngine.GameOptions.SystemTypes.Sabotage,
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
            UnityEngine.GameOptions.SystemTypes.Reactor,
            0
        );
    }

    public static void RepairOxygen()
    {
        RepairSystem(
            UnityEngine.GameOptions.SystemTypes.LifeSupp,
            0
        );
    }

    public static void RepairLights()
    {
        RepairSystem(
            UnityEngine.GameOptions.SystemTypes.Electrical,
            0
        );
    }

    public static void RepairComms()
    {
        RepairSystem(
            UnityEngine.GameOptions.SystemTypes.Comms,
            0
        );
    }

    private static void RepairSystem(
        UnityEngine.GameOptions.SystemTypes systemType,
        int amount)
    {
        try
        {
            var shipStatus = ShipStatus.Instance;
            if (shipStatus == null) return;

            shipStatus.RpcUpdateSystem(systemType, amount);
        }
        catch (System.Exception ex)
        {
            SickoMenuPlugin.PluginLogger.LogError(
                $"Failed to repair {systemType}: {ex.Message}");
        }
    }
}
