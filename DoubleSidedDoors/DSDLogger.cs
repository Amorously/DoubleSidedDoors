using BepInEx.Logging;

namespace DoubleSidedDoors;

internal static class DSDLogger
{
    private readonly static ManualLogSource _logger = Logger.CreateLogSource(EntryPoint.NAME);

    public static void Log(string str) => _logger.Log(LogLevel.Message, str);

    public static void Warn(string str) => _logger.Log(LogLevel.Warning, str);

    public static void Error(string str) => _logger.Log(LogLevel.Error, str);

    public static void Debug(string str, bool force = false)
    {
        if (Utils.ConfigManager.Debug || force)
            _logger.Log(LogLevel.Debug, str);
    }
}
