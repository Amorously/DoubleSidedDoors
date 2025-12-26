using AmorLib.Utils.JsonElementConverters;
using GameData;
using LevelGeneration;
using System.Text.Json.Serialization;

namespace DSD.Module;

public sealed class RearDoorConfigDefinition
{
    [JsonPropertyName("RearHandleTextActiveOverrides")]
    public eDoorStatus[] RearHandleTextOverrides { get; set; } = Array.Empty<eDoorStatus>();
    public uint ChainedPuzzleToEnter { get; set; } = 0;
    public BoolBase PlayScannerVoiceAudio { get; set; } = BoolBase.Unchanged;
    public BoolBase UseStaticBioscanPointsInZone { get; set; } = BoolBase.Unchanged;
    public ProgressionPuzzleData ProgressionPuzzleToEnter { get; set; } = new();
    public bool TurnOffAlarmOnTerminal { get; set; } = false;
    public TerminalZoneSelectionData TerminalPuzzleZone { get; set; } = new();
    public List<WardenObjectiveEventData> EventsOnApproach { get; set; } = new();
    public List<WardenObjectiveEventData> EventsOnUnlockDoor { get; set; } = new();
    public List<WardenObjectiveEventData> EventsOnDoorScanStart { get; set; } = new();
    public List<WardenObjectiveEventData> EventsOnDoorScanDone{ get; set; } = new();
    public List<WardenObjectiveEventData> EventsOnTerminalDeactivateAlarm { get; set; } = new();
    public DoorTriggerOverride RearTriggerOverride { get; set; } = new();
}
