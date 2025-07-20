using ChainedPuzzles;
using GameData;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;
using static DoubleSidedDoors.Module.SharedDoorData;
using Il2CppStringList = Il2CppSystem.Collections.Generic.List<string>;

namespace DoubleSidedDoors.Module.Patches
{
    [HarmonyPatch]
    internal static class Patch_SecurityDoorMisc
    {
        [HarmonyPatch(typeof(LG_SecurityDoor), nameof(LG_SecurityDoor.SetNavInfo))]
        [HarmonyPrefix]
        private static bool SecDoor_NavInfo(LG_SecurityDoor __instance, string infoFwd, string infoBwd, Il2CppStringList infoFwdClean, Il2CppStringList infoBwdClean)
        {
            if (!TryGetCurrentCustomConfig(cfg => cfg.SecDoorInstanceID, __instance.GetInstanceID(), out var custom) || !custom.Flipped) return true;

            __instance.m_graphics.SetNavInfoFwd(infoBwd);
            __instance.m_graphics.SetNavInfoBwd(infoFwd);
            __instance.m_terminalNavInfoForward = infoBwdClean;
            __instance.m_terminalNavInfoBackward = infoFwdClean;

            return false;
        }

        [HarmonyPatch(typeof(ChainedPuzzleManager), nameof(ChainedPuzzleManager.CreatePuzzleInstance), new Type[] 
        {
            typeof(ChainedPuzzleDataBlock),
            typeof(LG_Area),
            typeof(LG_Area),
            typeof(Vector3),
            typeof(Transform),
            typeof(bool)
        })]
        [HarmonyPrefix]
        private static void CreateChainedPuzzle(ref LG_Area sourceArea, ref LG_Area targetArea, Transform parent)
        {
            if (TryGetCurrentCustomConfig(cfg => cfg.DoorTransformInstanceID, parent.GetInstanceID(), out var custom) && custom.Flipped)
            {
                (targetArea, sourceArea) = (sourceArea, targetArea);
            }
        }

        [HarmonyPatch(typeof(LG_ZoneExpander), nameof(LG_ZoneExpander.GetOppositeArea))]
        [HarmonyPrefix]
        private static bool GetOppositeArea(LG_ZoneExpander __instance, ref LG_Area __result, LG_Area area)
        {
            if (OnDoorSync)
            {
                LG_Gate? gate = __instance.GetGate();
                if (gate != null && TryGetCurrentCustomConfig(cfg => cfg.GateInstanceID, gate.GetInstanceID(), out var custom) && custom.Flipped)
                {
                    __result = area;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(LG_GenericTerminalItem), nameof(LG_GenericTerminalItem.SpawnNode), MethodType.Setter)]
        [HarmonyPostfix]
        private static void SetSpawnNode(LG_GenericTerminalItem __instance)
        {
            if (TryGetCurrentCustomConfig(cfg => cfg.TerminalItemInstanceID, __instance.GetInstanceID(), out var custom) && custom.Flipped)
            {
                LG_Area area = custom.Identifier.AreaGateLinksTo;
                __instance.FloorItemLocation = area.m_zone.NavInfo.GetFormattedText(LG_NavInfoFormat.Full_And_Number_With_Underscore);
                __instance.m_spawnNode = area.m_courseNode;
            }
        }
    }
}
