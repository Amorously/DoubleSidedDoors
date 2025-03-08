using ChainedPuzzles;
using GameData;
using GTFO.API;
using LevelGeneration;
using SNetwork;
using static DoubleSidedDoors.Module.SharedDoorData;
using LinkedTrigger = DoubleSidedDoors.Module.DoorTriggerOverride.LinkedTriggerType;

namespace DoubleSidedDoors.Module;

internal class DoorBehavior
{
    internal static void Init()
    {
        LevelAPI.OnBuildStart += () =>
        {
            uint layout = RundownManager.ActiveExpedition.LevelLayoutData;
            Current = DSDData.TryGetValue(layout, out var value) ? value.Doors : Array.Empty<DSDCustomization>();
        };

        LevelAPI.OnBuildDone += SetupLinkedDoors;
    }

    private static void SetupLinkedDoors()
    {
        foreach (var custom in Current)
        {
            OverrideIntText(custom.Identifier.SecDoor.m_locks.TryCast<LG_SecurityDoor_Locks>());

            if (TryGetDoorFromGlobalIndex(custom.TriggerOverride.Global, out var linkedDoor))
            {                
                bool isSource = custom.TriggerOverride.LinkTriggerTo == LinkedTrigger.Source;
                var sourceDoor = isSource ? custom.Identifier.SecDoor : linkedDoor;
                var targetDoor = isSource ? linkedDoor : custom.Identifier.SecDoor;
                custom.TriggerOverride.LinkedDoor = linkedDoor;

                if (custom.TriggerOverride.LinkTriggerTo == LinkedTrigger.Source || custom.TriggerOverride.LinkTriggerTo == LinkedTrigger.Target)
                {
                    pDoorState activeState = new() { status = eDoorStatus.ChainedPuzzleActivated };
                    LG_SecurityDoor_Locks locks = targetDoor.m_locks.Cast<LG_SecurityDoor_Locks>();     
                    
                    if (sourceDoor.m_locks.ChainedPuzzleToSolve == null)
                    {
                        DSDLogger.Warn($"Hey, door {(isSource ? custom.PrintGlobal() : custom.TriggerOverride.PrintGlobal())} for linked trigger to door {(isSource ? custom.TriggerOverride.PrintGlobal() : custom.PrintGlobal())} does not have a chained puzzle! Skipping linking these doors");
                        continue;
                    }
                    else if (targetDoor.m_locks.ChainedPuzzleToSolve == null)
                    {
                        bool sourceHasAlarm = ChainedPuzzleDataBlock.GetBlock(sourceDoor.LinkedToZoneData.ChainedPuzzleToEnter).TriggerAlarmOnActivate;
                        targetDoor.SetupChainedPuzzleLock(sourceHasAlarm ? 7u : 4u);
                    }

                    sourceDoor.m_locks.add_OnPlayerActivateChainedPuzzle((Action)(() =>
                    {
                        targetDoor.m_graphics.OnDoorState(activeState, false);
                        targetDoor.m_anim.OnDoorState(activeState, false);
                        locks.m_intUseKeyItem.SetActive(false);
                        locks.m_intOpenDoor.SetActive(false);
                        locks.m_intHack.SetActive(false);
                        locks.m_intCustomMessage.SetActive(false);
                    }));                    

                    targetDoor.m_locks.add_OnPlayerActivateChainedPuzzle((Action)(() =>
                    {
                        sourceDoor.m_sync.AttemptDoorInteraction(eDoorInteractionType.ActivateChainedPuzzle);
                        targetDoor.m_graphics.OnDoorState(activeState, false);
                        targetDoor.m_anim.OnDoorState(activeState, false);
                        locks.m_intUseKeyItem.SetActive(false);
                        locks.m_intOpenDoor.SetActive(false);
                        locks.m_intHack.SetActive(false);
                        locks.m_intCustomMessage.SetActive(false);
                    }));

                    sourceDoor.m_locks.add_OnChainedPuzzleSolved((Action)(() =>
                    {
                        pDoorState unlockState = new() { status = eDoorStatus.Unlocked };
                        targetDoor.m_graphics.OnDoorState(unlockState, false);
                        targetDoor.m_anim.OnDoorState(unlockState, false);
                        targetDoor.m_locks.OnDoorState(unlockState, false);

                        pChainedPuzzleState solvedCPState = targetDoor.m_locks.ChainedPuzzleToSolve!.m_stateReplicator.State;
                        solvedCPState.isSolved = true;
                        solvedCPState.isActive = false;
                        if (SNet.IsMaster) targetDoor.m_locks.ChainedPuzzleToSolve.m_stateReplicator.State = solvedCPState;
                        targetDoor.m_sync.SetStateUnsynced(unlockState);
                    }));
                }
                else if (custom.TriggerOverride.LinkTriggerTo == LinkedTrigger.Both)
                {
                    sourceDoor.m_locks.add_OnPlayerActivateChainedPuzzle((Action)(() => targetDoor.m_sync.AttemptDoorInteraction(eDoorInteractionType.ActivateChainedPuzzle)));
                    targetDoor.m_locks.add_OnPlayerActivateChainedPuzzle((Action)(() => sourceDoor.m_sync.AttemptDoorInteraction(eDoorInteractionType.ActivateChainedPuzzle)));
                }

                if (custom.TriggerOverride.OpenOnTarget)
                {
                    sourceDoor.m_anim.add_OnDoorIsOpen((Action)(() => targetDoor.m_sync.AttemptDoorInteraction(eDoorInteractionType.Open)));
                    targetDoor.m_anim.add_OnDoorIsOpen((Action)(() => sourceDoor.m_sync.AttemptDoorInteraction(eDoorInteractionType.Open)));
                }
            }
        }
    }
}
