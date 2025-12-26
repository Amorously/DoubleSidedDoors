using HarmonyLib;
using LevelGeneration;
using UnityEngine;
using static DSD.Module.DoorConfigManager;
using LinkedTrigger = DSD.Module.DoorTriggerOverride.LinkedTriggerType;

namespace DSD.Patches;

[HarmonyPatch]
internal static class SecDoorSyncPatches
{
    private readonly static string[] _secDoorDisplayGraphics = {
        "Security_Display_ScanActive",
        "Security_Display_Locked",
        "Security_Display_LockedAlarm",
        "Security_Display_UnLocked"
    };

    [HarmonyPatch(typeof(LG_Door_Graphics), nameof(LG_Door_Graphics.OnDoorState))]
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    private static void Graphic_OnDoorState(LG_Door_Graphics __instance, ref pDoorState state)
    {
        LG_SecurityDoor? door = __instance.m_core.TryCast<LG_SecurityDoor>();
        if (door == null || !TryGetCurrentCustomConfig(cfg => cfg.SecDoorInstanceID, door.GetInstanceID(), out var custom)) return;
        if (custom.GraphicStateOverride == eDoorStatus.None || !state.status.ToString().Contains("Closed_LockedWith")) return;
        if ((state.status = custom.GraphicStateOverride) != eDoorStatus.Destroyed) return;

        // Note(randomuserhi): Can instead patch setup and add key to m_graphicalModeLookup with correct objects to hide => See GraphicalModes and LG_Door_Graphics.m_graphicalModeLookup

        foreach (string securityDisplay in _secDoorDisplayGraphics)
        {
            Transform? displayTransform = door.m_doorBladeCuller.transform.Find($"securityDoor_8x4_tech/bottomDoor/{securityDisplay}")
                ?? door.m_doorBladeCuller.transform.Find($"securityDoor_4x4_tech/rightDoor/{securityDisplay}");
            displayTransform?.gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(LG_SecurityDoor_Locks), nameof(LG_SecurityDoor_Locks.OnDoorState))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.High)]
    [HarmonyWrapSafe]
    private static void InteractText_OnDoorState(LG_SecurityDoor_Locks __instance, pDoorState state)
    {
        if (state.status != eDoorStatus.Closed_LockedWithChainedPuzzle && state.status != eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm) return;

        OverrideIntText(__instance);
    }

    [HarmonyPatch(typeof(LG_SecurityDoor), nameof(LG_SecurityDoor.OnSyncDoorStatusChange))]
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    private static bool Prefix_OnSyncDoorStatusChange(LG_SecurityDoor __instance, pDoorState state)
    {
        OnDoorSync = true;

        if (!TryGetCurrentCustomConfig(cfg => cfg.SecDoorInstanceID, __instance.GetInstanceID(), out var custom)) return true;

        if (state.status == eDoorStatus.Open || state.status == eDoorStatus.Opening)
        {
            custom.Identifier.RearGateInteract?.SetActive(false);
        }

        return state.status != eDoorStatus.ChainedPuzzleActivated || custom.TriggerOverride.LinkTriggerTo != LinkedTrigger.Target;
    }

    [HarmonyPatch(typeof(LG_SecurityDoor), nameof(LG_SecurityDoor.OnSyncDoorStatusChange))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]   
    private static void Postfix_OnSyncDoorStatusChange()
    {
        OnDoorSync = false;
    }
}