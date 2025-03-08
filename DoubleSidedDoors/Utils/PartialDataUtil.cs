using BepInEx.Unity.IL2CPP;
using System.Text.Json.Serialization;

namespace DoubleSidedDoors.Utils;

// Pretty much entirely yoinked from Jarhead's LogLibrary which was yoinked from Dinorush's ExtraWeaponCustomization
internal static class PartialDataUtil
{
    public const string PLUGIN_GUID = "MTFO.Extension.PartialBlocks";
    public readonly static bool HasPData;

    public static JsonConverter? PersistentIDConverter { get; private set; } = null;

    static PartialDataUtil()
    {
        if (IL2CPPChainloader.Instance.Plugins.TryGetValue(PLUGIN_GUID, out var info))
        {
            try
            {
                var ddAsm = info?.Instance?.GetType()?.Assembly ?? throw new Exception("Assembly is missing!");
                var types = ddAsm.GetTypes();
                var converterType = types.First(t => t.Name == "PersistentIDConverter") ?? throw new Exception("Unable to find PersistentIDConverter class");
                PersistentIDConverter = (JsonConverter)Activator.CreateInstance(converterType)!;
                HasPData = true;
            }
            catch (Exception e)
            {
                DSDLogger.Error($"Exception thrown while reading data from MTFO_Extension_PartialData:\n{e}");
            }
        }
    }
}