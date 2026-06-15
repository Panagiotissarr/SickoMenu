namespace SickoMenu.Utils;

public static class Logger
{
    public static void Info(string message) =>
        SickoMenuPlugin.PluginLogger.LogInfo(message);

    public static void Warn(string message) =>
        SickoMenuPlugin.PluginLogger.LogWarning(message);

    public static void Error(string message) =>
        SickoMenuPlugin.PluginLogger.LogError(message);

    public static void Debug(string message) =>
        SickoMenuPlugin.PluginLogger.LogDebug(message);

    public static void Log(string message) =>
        Info(message);

    public static void LogError(string message) =>
        Error(message);

    public static void LogWarning(string message) =>
        Warn(message);

    public static void LogDebug(string message) =>
        Debug(message);

    public static void Info(string format, params object[] args) =>
        Info(string.Format(format, args));

    public static void Error(string format, params object[] args) =>
        Error(string.Format(format, args));

    public static void Warn(string format, params object[] args) =>
        Warn(string.Format(format, args));
}
