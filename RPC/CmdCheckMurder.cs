namespace SickoMenu.RPC;

public static class CmdCheckMurderHandler
{
    public static bool HandleMurderCommand(PlayerControl killer, PlayerControl target)
    {
        if (killer == null || target == null) return false;

        SickoMenuPlugin.PluginLogger.LogInfo(
            $"CmdCheckMurder: P{killer.PlayerId} -> P{target.PlayerId}");

        Events.KillEventHandler.HandleMurder(killer.PlayerId, target.PlayerId);

        return true;
    }
}

public static class CmdCheckMurderResultHandler
{
    public static bool Handle(PlayerControl killer, PlayerControl target)
    {
        return CmdCheckMurderHandler.HandleMurderCommand(killer, target);
    }
}
