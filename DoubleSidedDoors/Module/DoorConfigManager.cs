using AmorLib.Utils.Extensions;
using GTFO.API;
using GTFO.API.Utilities;
using MTFO.API;

namespace DSD.Module;

public static partial class DoorConfigManager
{
    public static DSDCustomization[] Current { get; private set; } = Array.Empty<DSDCustomization>();
    public static string ModulePath { get; private set; } = string.Empty;

    private static readonly Dictionary<string, HashSet<uint>> _filepathDoorMap = new();
    private static readonly Dictionary<uint, DoorConfigDefinition> _customDoorData = new();    
    
    static DoorConfigManager()
    {
        ModulePath = Path.Combine(MTFOPathAPI.CustomPath, "DoubleSidedDoors");

        foreach (string customFile in Directory.EnumerateFiles(ModulePath, "*.json", SearchOption.AllDirectories))
        {
            string content = File.ReadAllText(customFile);
            ReadFileContent(customFile, content);
        }

        if (Configuration.UseLiveEdit)
        {
            LiveEditListener listener = LiveEdit.CreateListener(ModulePath, "*.json", false);
            listener.FileChanged += FileCreatedOrChanged;
            listener.FileCreated += FileCreatedOrChanged;
            listener.FileDeleted += FileDeleted;
        }

        LevelAPI.OnBuildStart += OnBuildStart;
        LevelAPI.OnBuildDone += OnBuildDone;
    }

    private static void ReadFileContent(string file, string content)
    {
        var layoutSet = _filepathDoorMap.GetOrAddNew(file);

        foreach (uint id in layoutSet)
        {
            _customDoorData.Remove(id);
        }
        layoutSet.Clear();

        foreach (var data in DSDJson.Deserialize<IEnumerable<DoorConfigDefinition>>(content))
        {
            if (data != null && data.MainLevelLayout != 0u)
            {
                layoutSet.Add(data.MainLevelLayout);
                _customDoorData[data.MainLevelLayout] = data;
            }
        }
    }

    private static void FileCreatedOrChanged(LiveEditEventArgs e)
    {
        DSDLogger.Warn($"LiveEdit file changed: {e.FullPath}");
        LiveEdit.TryReadFileContent(e.FullPath, (content) =>
        {
            ReadFileContent(e.FullPath, content);
        });
    }

    private static void FileDeleted(LiveEditEventArgs e)
    {
        DSDLogger.Warn($"LiveEdit File deleted: {e.FullPath}");
        LiveEdit.TryReadFileContent(e.FullPath, (content) =>
        {
            foreach (uint id in _filepathDoorMap[e.FullPath])
            {
                _customDoorData.Remove(id);
            }
            _filepathDoorMap.Remove(e.FullPath);
        });
    }

    private static void OnBuildStart()
    {
        uint layout = RundownManager.ActiveExpedition.LevelLayoutData;
        Current = _customDoorData.TryGetValue(layout, out var value) ? value.Doors : Array.Empty<DSDCustomization>();
    }

    private static void OnBuildDone()
    {
        SetupLinkedDoors();
    }    
}
