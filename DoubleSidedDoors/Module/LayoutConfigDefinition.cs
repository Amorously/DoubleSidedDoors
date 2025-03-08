using DoubleSidedDoors.Utils;
using GameData;
using LevelGeneration;
using System.Text.Json.Serialization;
using UnityEngine;

namespace DoubleSidedDoors.Module;

public sealed class LayoutConfig
{
    [JsonIgnore]
    public string Filepath { get; set; } = string.Empty; 
    public uint MainLevelLayout { get; set; }
    public DSDCustomization[] Doors { get; set; } = Array.Empty<DSDCustomization>();
}

public sealed class DSDCustomization : GlobalIndexBase
{
    public DSDType Type { get; set; } = DSDType.Flipped;
    public HashSet<HashSet<int>> BindToPlayer { get; set; } = new();
    public LocaleText FrontHandleText { get; set; } = LocaleText.Empty;
    public LocaleText RearHandleText { get; set; } = (LocaleText)"<color=red>BI-DIRECTIONAL ACCESS DISABLED</color>";
    public DoorTriggerOverride TriggerOverride { get; set; } = new();
    public eDoorStatus GraphicStateOverride { get; set; } = eDoorStatus.None;

    [JsonIgnore]
    public DoorIdentifier Identifier { get; set; } = new();
    public bool Flipped => Type.ToString().Contains("Flipped");
    public bool DoubleHandle => Type.ToString().Contains("Double");
    public enum DSDType : byte
    {
        Flipped,
        DoubleHandleOnly,
        OverrideOnly,
        DoubleSided, // not actually interactable on both sides, yet
        FlippedDoubleSided
    }
}

public sealed class DoorTriggerOverride : GlobalIndexBase
{
    public bool OpenOnTarget { get; set; } = false;
    public LinkedTriggerType LinkTriggerTo {  get; set; } = LinkedTriggerType.None;
    [JsonIgnore]
    public LG_SecurityDoor LinkedDoor { get; set; } = new();
    public enum LinkedTriggerType : byte
    {
        None,
        Source,
        Target,
        Both
    }
}

public sealed class DoorIdentifier
{
    public LG_SecurityDoor SecDoor { get; private set; } = new();
    public GameObject? RearGateInteract { get; private set; } = new();
    public LG_Area AreaGateLinksTo { get; private set; } = new();
    public int SecDoorInstanceID { get; private set; }
    public int DoorTransformInstanceID { get; private set; }
    public int GateInstanceID { get; private set; }        
    public int TerminalItemInstanceID { get; private set; }

    public void Initialize(LG_SecurityDoor door, LG_Gate gate, GameObject? rearInteract)
    {
        SecDoor = door;
        RearGateInteract = rearInteract;
        AreaGateLinksTo = gate.m_linksTo;
        SecDoorInstanceID = door.GetInstanceID();
        DoorTransformInstanceID = door.transform.GetInstanceID();
        GateInstanceID = gate.GetInstanceID();            
        TerminalItemInstanceID = door.m_terminalItem.Cast<LG_GenericTerminalItem>().GetInstanceID();            
    }
}

public class GlobalIndexBase
{
    public GlobalZoneIndex Global { get => new(DimensionIndex, Layer, LocalIndex); }
    public LG_LayerType Layer { get; set; } = LG_LayerType.MainLayer;
    public eDimensionIndex DimensionIndex { get; set; } = eDimensionIndex.Reality;
    public eLocalZoneIndex LocalIndex { get; set; } = eLocalZoneIndex.Zone_0;
    
    public string PrintGlobal()
    {
        return $"(Dimension: {DimensionIndex}, Layer: {Layer}, LocalIndex: {LocalIndex})";
    }
}
