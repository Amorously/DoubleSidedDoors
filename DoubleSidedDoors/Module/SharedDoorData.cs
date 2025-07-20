using DoubleSidedDoors.Utils;
using LevelGeneration;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace DoubleSidedDoors.Module;

public static class SharedDoorData
{
    public static Dictionary<uint, LayoutConfig> DSDData { get; set; } = new();
    public static DSDCustomization[] Current { get; set; } = Array.Empty<DSDCustomization>();
    public static bool OnDoorSync { get; set; } = false;

    public static GlobalZoneIndex GetGlobalIndexFrom(LG_Gate gate)
    {
        return new()
        {
            Dimension = gate.m_linksTo.m_zone.DimensionIndex,
            Layer = gate.m_linksTo.m_zone.Layer.m_type,
            Zone = gate.m_linksTo.m_zone.LocalIndex
        };
    }

    public static bool TryGetCurrentCustomConfig(Func<DoorIdentifier, int> property, int id, [NotNullWhen(true)] out DSDCustomization? custom)
    {
        custom = Current.FirstOrDefault(cfg => property(cfg.Identifier) == id);
        return custom != null;
    }

    public static bool TryGetDoorFromGlobalIndex(GlobalZoneIndex global, [NotNullWhen(true)] out LG_SecurityDoor? door)
    {
        if (Builder.CurrentFloor.TryGetZoneByLocalIndex(global.Dimension, global.Layer, global.Zone, out LG_Zone zone))
        {
            door = zone?.m_sourceGate?.SpawnedDoor?.TryCast<LG_SecurityDoor>();
            return door != null;
        }
        door = null;
        return false;
    }

    public static void OverrideIntText(LG_SecurityDoor_Locks? locks)
    {
        if (locks == null || !TryGetCurrentCustomConfig(cfg => cfg.SecDoorInstanceID, locks.m_door.GetInstanceID(), out var custom) || custom.FrontHandleText == LocaleText.Empty) return;

        string str = Regex.Replace(custom.FrontHandleText.ToString(), @"\[DOOR_(\d+)_(\d+)_(\d+)\]", m => {
            int dimIndex = int.Parse(m.Groups[1].Value);
            int layerType = int.Parse(m.Groups[2].Value);
            int localIndex = int.Parse(m.Groups[3].Value);

            if (TryGetDoorFromGlobalIndex(new(dimIndex, layerType, localIndex), out var door))
            {
                return $"<color=orange>SEC_DOOR_{door.m_serialNumber}</color>";
            }
            return $"<color=white>[DSD - DEBUG] DOOR({dimIndex}_{layerType}_{localIndex}) not found</color>";
        });

        locks.m_intOpenDoor.InteractionMessage = str;
        locks.m_intCustomMessage.m_message = str;
    }
}
