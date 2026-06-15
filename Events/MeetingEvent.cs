namespace SickoMenu.Events;

public static class MeetingEventHandler
{
    public static void HandleMeetingStart(byte reporterId, byte? bodyId)
    {
        var data = new Dictionary<string, object>
        {
            ["BodyId"] = bodyId ?? -1
        };

        GameEventBus.Fire(GameEventType.MeetingStart, reporterId, data: data);
        SickoMenuPlugin.PluginLogger.LogInfo(
            $"Meeting called by P{reporterId}" +
            (bodyId.HasValue ? $" (body: P{bodyId})" : ""));
    }

    public static void HandleMeetingEnd()
    {
        GameEventBus.Fire(GameEventType.MeetingEnd, 0);
        SickoMenuPlugin.PluginLogger.LogInfo("Meeting ended");
    }

    public static void HandleVote(byte voterId, byte suspectId)
    {
        var data = new Dictionary<string, object>
        {
            ["SuspectId"] = suspectId
        };

        GameEventBus.Fire(GameEventType.Custom, voterId, suspectId, data);
    }

    public static void HandleReport(byte reporterId, byte bodyId)
    {
        var data = new Dictionary<string, object>
        {
            ["BodyId"] = bodyId
        };

        GameEventBus.Fire(GameEventType.ReportBody, reporterId, bodyId, data);
    }
}
