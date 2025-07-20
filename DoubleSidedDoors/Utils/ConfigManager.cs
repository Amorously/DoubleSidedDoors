using BepInEx;
using BepInEx.Configuration;
using DoubleSidedDoors.Module;
using GTFO.API.Utilities;
using MTFO.API;

namespace DoubleSidedDoors.Utils;

internal static class ConfigManager
{
    private readonly static ConfigFile _configFile;
    private readonly static ConfigEntry<bool> _debug;
    private readonly static string CustomPath = Path.Combine(MTFOPathAPI.CustomPath, "DoubleSidedDoors");
    public static bool Debug { get => _debug.Value; set => _debug.Value = value; }

    static ConfigManager()
    {            
        _configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, EntryPoint.NAME + ".cfg"), true);
        _debug = _configFile.Bind("Debug", "enable", false, "Force enables all debugging messages in the console.");

        DirectoryInfo dir = Directory.CreateDirectory(CustomPath);
        FileInfo[] files = dir.GetFiles();
        DSDLogger.Debug($"Searching: {dir.FullName}", true);

        foreach (FileInfo fileInfo in files)
        {
            DSDLogger.Debug($"Found: {fileInfo.FullName}", true);
            string extension = fileInfo.Extension;
            bool isJson = extension.Equals(".json", StringComparison.InvariantCultureIgnoreCase);
            bool isJsonc = extension.Equals(".jsonc", StringComparison.InvariantCultureIgnoreCase);

            if (isJson || isJsonc)
            {
                LayoutConfig layoutConfig = JSON.Deserialize<LayoutConfig>(File.ReadAllText(fileInfo.FullName));
                layoutConfig.Filepath = fileInfo.FullName;
                if (!SharedDoorData.DSDData.TryAdd(layoutConfig.MainLevelLayout, layoutConfig))
                {
                    DSDLogger.Error($"Duplicated ID found!: {fileInfo.Name}, {layoutConfig.MainLevelLayout}");
                }
            }
        }

        if (PartialDataUtil.HasPData)
        {
            LiveEditListener listener = LiveEdit.CreateListener(CustomPath, "*.json", false);
            listener.FileChanged += FileChanged;
            listener.FileCreated += FileChanged;
            listener.FileDeleted += FileDeleted;
        }
    }

    private static void FileChanged(LiveEditEventArgs e)
    {
        DSDLogger.Warn($"[LiveEdit] File changed: {e.FullPath}");
        LiveEdit.TryReadFileContent(e.FullPath, (content) =>
        {
            LayoutConfig layoutConfig = JSON.Deserialize<LayoutConfig>(content);
            layoutConfig.Filepath = e.FullPath;
            SharedDoorData.DSDData[layoutConfig.MainLevelLayout] = layoutConfig;
        });
    }

    private static void FileDeleted(LiveEditEventArgs e)
    {
        DSDLogger.Warn($"[LiveEdit] File deleted: {e.FullPath}");
        foreach (var data in SharedDoorData.DSDData.Where(d => d.Value.Filepath == e.FullPath))
        {
            SharedDoorData.DSDData.Remove(data.Key);
        }
    }
}