namespace SickoMenu.RPC;

public static class RpcCompleteTaskHandler
{
    public static void Handle(PlayerControl player, uint taskIdx)
    {
        if (player == null || player.Data?.Tasks == null) return;

        if (taskIdx < player.Data.Tasks.Count)
        {
            var task = player.Data.Tasks[(int)taskIdx];
            if (task != null)
            {
                task.Complete = true;
                SickoMenuPlugin.PluginLogger.LogInfo(
                    $"Task completed: P{player.PlayerId} task #{taskIdx}");

                Events.TaskEventHandler.HandleTaskComplete(
                    player.PlayerId, taskIdx, task.TaskType.ToString());

                CheckAllTasksComplete(player);
            }
        }
    }

    public static void HandleCompleteAll(PlayerControl player)
    {
        if (player == null || player.Data?.Tasks == null) return;

        for (int i = 0; i < player.Data.Tasks.Count; i++)
        {
            var task = player.Data.Tasks[i];
            if (task != null && !task.Complete)
            {
                task.Complete = true;
                SickoMenuPlugin.PluginLogger.LogInfo(
                    $"Task auto-completed: P{player.PlayerId} task #{i}");
            }
        }

        Events.TaskEventHandler.HandleAllTasksComplete(player.PlayerId);
    }

    private static void CheckAllTasksComplete(PlayerControl player)
    {
        if (player.Data?.Tasks == null) return;

        bool allComplete = true;
        for (int i = 0; i < player.Data.Tasks.Count; i++)
        {
            var task = player.Data.Tasks[i];
            if (task != null && !task.Complete)
            {
                allComplete = false;
                break;
            }
        }

        if (allComplete)
            Events.TaskEventHandler.HandleAllTasksComplete(player.PlayerId);
    }
}
