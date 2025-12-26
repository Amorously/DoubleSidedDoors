using AmorLib.Networking.StateReplicators;
using ChainedPuzzles;
using GameData;
using Il2CppInterop.Runtime.Attributes;
using LevelGeneration;
using UnityEngine;

namespace DSD.Module;

internal enum 

internal struct RearDoorState
{
    public eDoorStatus status;
    public bool hasBeenApproached;
    public bool isOpen;
    public bool hasBeenOpened;
}

internal class RearDoorReplicator : MonoBehaviour, IStateReplicatorHolder<RearDoorState>
{
    [HideFromIl2Cpp]
    public StateReplicator<RearDoorState>? Replicator { get; private set; }
    public RearDoorConfigDefinition RearConfig;
    public LG_SecurityDoor? Door;
    public GameObject RearHandle;
    public Transform BioscanAlign;
    public ChainedPuzzleInstance? ChainedPuzzle;
    public eDoorStatus Status, LastStatus;
    public bool HasBeenApproached = false;
    public bool IsOpen = false;
    public bool HasBeenOpened = false;
    // animator
    // graphics    
    // interactions 
    public Queue<WardenObjectiveEventData> EventsOnApproach = new();
    public Queue<WardenObjectiveEventData> EventsOnUnlockDoor = new();
    public Queue<WardenObjectiveEventData> EventsOnDoorScanStart = new();
    public Queue<WardenObjectiveEventData> EventsOnDoorScanDone = new();
    public Queue<WardenObjectiveEventData> EventsOnTerminalDeactivateAlarm = new();

    public void Setup(RearDoorConfigDefinition config)
    {
        RearConfig = config;
    }

    public void OnAwake()
    {
        Door = GetComponent<LG_SecurityDoor>();
    }

    public void OnEnable()
    {

    }

    public void OnDestroy()
    {
        Replicator?.Unload();
    }

    public void OnStateChange(RearDoorState oldState, RearDoorState state, bool isRecall)
    {
       
    }
}
