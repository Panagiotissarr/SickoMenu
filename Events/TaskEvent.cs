namespace SickoMenu.Events;

public static class TaskEventHandler
{
    public static void HandleTaskComplete(byte playerId, uint taskIdx, string taskType)
    {
        var data = new Dictionary<string, object>
        {
            ["TaskIndex"] = (int)taskIdx,
            ["TaskType"] = taskType
        };

        GameEventBus.Fire(GameEventType.TaskComplete, playerId, data: data);
        SickoMenuPlugin.PluginLogger.LogInfo(
            $"Task completed: P{playerId} task #{taskIdx} ({taskType})");
    }

    public static void HandleAllTasksComplete(byte playerId)
    {
        var data = new Dictionary<string, object>
        {
            ["AllComplete"] = true
        };

        GameEventBus.Fire(GameEventType.Custom, playerId, data: data);
        SickoMenuPlugin.PluginLogger.LogInfo($"All tasks completed: P{playerId}");
    }
}
