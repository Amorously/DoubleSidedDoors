using BepInEx;
using BepInEx.Configuration;
using GTFO.API.Utilities;

namespace DSD;

internal static class Configuration
{
    private static readonly ConfigFile _configFile;
    private static readonly ConfigEntry<bool> _debug;

    public static bool UseVerboseLogs => _configUseVerboseLogs.Value;
    private static readonly ConfigEntry<bool> _configUseVerboseLogs;

    public static bool UseLiveEdit => _configUseLiveEdit.Value;
    private static readonly ConfigEntry<bool> _configUseLiveEdit;   

    public static bool CreateTemplate => _configCreateTemplate.Value;
    private static readonly ConfigEntry<bool> _configCreateTemplate;

    static Configuration()
    {            
        _configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "DoubleSidedDoors.cfg"), true);

        string section = "Settings";
        _debug = _configFile.Bind(section, "enable", false, "Deprecated config, does nothing.");
        _configUseVerboseLogs = _configFile.Bind(section, "Enable verbose logging", false, "Print additional debug logs to the console.");
        _configUseLiveEdit = _configFile.Bind(section, "Use LiveEdit", true, "Enable LiveEdit for custom datablocks.");
        _configCreateTemplate = _configFile.Bind(section, "Create Template", true, "Create a template file.");

        LiveEditListener configListener = LiveEdit.CreateListener(Paths.ConfigPath, "DoubleSidedDoors.cfg", false);
        configListener.FileChanged += ConfigFileChanged;        
    }

    private static void ConfigFileChanged(LiveEditEventArgs e)
    {
        DSDLogger.Warn($"Config file changed: {e.FullPath}");
        _configFile.Reload();
    }
}