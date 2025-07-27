using AmorLib.Utils;
using AmorLib.Utils.JsonElementConverters;
using ChainedPuzzles;
using GameData;
using LevelGeneration;
using SNetwork;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using LinkedTrigger = DSD.Module.DoorTriggerOverride.LinkedTriggerType;

namespace DSD.Module;

public static partial class DoorConfigManager
{
    internal static bool OnDoorSync { get; set; } = false;

    public static GlobalZoneIndex GetGlobalIndexFrom(LG_Gate gate)
    {
        return gate.m_linksTo.m_zone.ToStruct();
    }

    public static bool TryGetCurrentCustomConfig(Func<DoorIdentifier, int> property, int id, [MaybeNullWhen(false)] out DSDCustomization custom)
    {
        custom = Current.FirstOrDefault(cfg => property(cfg.Identifier) == id);
        return custom != null;
    }

    public static bool TryGetDoorFromGlobalIndex((int, int, int) globalIndex, [MaybeNullWhen(false)] out LG_SecurityDoor door)
    {
        if (globalIndex.TryGetZone(out var zone))
        {
            door = zone?.m_sourceGate?.SpawnedDoor?.TryCast<LG_SecurityDoor>();
            return door != null;
        }
        door = null;
        return false;
    }

    internal static void OverrideIntText(LG_SecurityDoor_Locks? locks)
    {
        if (locks == null || !TryGetCurrentCustomConfig(cfg => cfg.SecDoorInstanceID, locks.m_door.GetInstanceID(), out var custom) || custom.FrontHandleText == LocaleText.Empty) return;

        string str = Regex.Replace(custom.FrontHandleText.ToString(), @"\[DOOR_(\d+)_(\d+)_(\d+)\]", m =>
        {
            int dimIndex = int.Parse(m.Groups[1].Value);
            int layerType = int.Parse(m.Groups[2].Value);
            int localIndex = int.Parse(m.Groups[3].Value);
            var globalIndex = (dimIndex, layerType, localIndex);

            if (TryGetDoorFromGlobalIndex(globalIndex, out var door))
            {
                return $"<color=orange>SEC_DOOR_{door.m_serialNumber}</color>";
            }
            return $"<color=white>[DSD - DEBUG] DOOR(D{dimIndex}_L{layerType}_Z{localIndex}) not found</color>";
        });

        if (!custom.FrontHandleTextOverrides.Any() || custom.FrontHandleTextOverrides.Contains(locks.m_lastStatus))
        {
            locks.m_intOpenDoor.InteractionMessage = str;
            locks.m_intCustomMessage.m_message = str;
        }
    }

    private static void SetupLinkedDoors()
    {
        foreach (var custom in Current)
        {
            OverrideIntText(custom.Identifier.SecDoor.m_locks.TryCast<LG_SecurityDoor_Locks>());
            if (!TryGetDoorFromGlobalIndex(custom.TriggerOverride.IntTuple, out var linkedDoor)) continue;
            
            bool isSource = custom.TriggerOverride.LinkTriggerTo == LinkedTrigger.Source;
            var sourceDoor = isSource ? custom.Identifier.SecDoor : linkedDoor;
            var targetDoor = isSource ? linkedDoor : custom.Identifier.SecDoor;
            custom.TriggerOverride.LinkedDoor = linkedDoor;

            if (custom.TriggerOverride.LinkTriggerTo == LinkedTrigger.Source || custom.TriggerOverride.LinkTriggerTo == LinkedTrigger.Target)
            {
                pDoorState activeState = new() { status = eDoorStatus.ChainedPuzzleActivated };
                var locks = targetDoor.m_locks.Cast<LG_SecurityDoor_Locks>();

                if (sourceDoor.m_locks.ChainedPuzzleToSolve == null)
                {
                    DSDLogger.Warn($"Source door {custom.ToString()} for linked trigger does not have a chained puzzle! Skipping linking these doors");
                    continue;
                }
                else if (targetDoor.m_locks.ChainedPuzzleToSolve == null)
                {
                    DSDLogger.Verbose("Adding chained puzzle to target door " + custom.TriggerOverride.ToString());
                    AddChainedPuzzleToTargetDoor(targetDoor, ChainedPuzzleDataBlock.GetBlock(sourceDoor.LinkedToZoneData.ChainedPuzzleToEnter).TriggerAlarmOnActivate);
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

    private static void AddChainedPuzzleToTargetDoor(LG_SecurityDoor targetDoor, bool sourceHasAlarm)
    {
        if (!targetDoor.GetChainedPuzzleStartPosition(out var pos)) return;
        
        var block = ChainedPuzzleDataBlock.GetBlock(sourceHasAlarm ? 7u : 4u);
        var puzzleInstance = ChainedPuzzleManager.CreatePuzzleInstance(block, targetDoor.Gate.ProgressionSourceArea, targetDoor.Gate.m_linksTo, pos, targetDoor.transform);
        var addedStatus = targetDoor.m_locks.SetupForChainedPuzzle(puzzleInstance);
        targetDoor.m_sync.SetStateUnsynced(new pDoorState { status = addedStatus });

        if (addedStatus == eDoorStatus.Closed || addedStatus == eDoorStatus.Unlocked)
        {
            if (block.TriggerAlarmOnActivate)
            {
                targetDoor.m_graphics.OnDoorState(new pDoorState { status = eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm }, false);
                targetDoor.m_mapLookatRevealer.SetLocalGUIObjStatus(eCM_GuiObjectStatus.DoorSecureApex);
            }
            else
            {
                targetDoor.m_graphics.OnDoorState(new pDoorState { status = eDoorStatus.Closed_LockedWithChainedPuzzle }, false);
            }
        }
    }
}
