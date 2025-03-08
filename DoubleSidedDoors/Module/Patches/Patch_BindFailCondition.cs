using HarmonyLib;
using LevelGeneration;
using Player;
using static DoubleSidedDoors.Module.SharedDoorData;

namespace DoubleSidedDoors.Module.Patches;

[HarmonyPatch]
internal static class Patch_BindFailCondition
{
    [HarmonyPatch(typeof(WardenObjectiveManager), nameof(WardenObjectiveManager.CheckExpeditionFailed))]
    [HarmonyPostfix]
    private static void CheckExpeditionFailed(ref bool __result)
    {
        if (__result) return;

        foreach (var custom in Current)
        {
            foreach (var bindSet in custom.BindToPlayer)
            {
                if (AllPlayersAreDownInSet(bindSet, out var deadPlayerNames) && custom.Identifier.SecDoor.LastStatus != eDoorStatus.Open)
                {
                    DSDLogger.Warn($"{string.Join(" + ", deadPlayerNames)} were downed while bound to unopened door -> ending run!");
                    __result = true;
                    return;
                }
            }
        }
    } 

    private static bool AllPlayersAreDownInSet(HashSet<int> set, out IEnumerable<string> deadPlayerNames)
    {
        deadPlayerNames = set.Where(PlayerIsDown).Select(DeadPlayerNames);

        var validSet = set.Select(GetPlayerFrom).Where(player => player != null).ToList();
        if (validSet.Count < 1)
        {
            return set.All(PlayerIsDown);
        }

        return validSet.All(player => !player!.Alive);
    }

    private static string DeadPlayerNames(int slot) => GetPlayerFrom(slot)?.PlayerName ?? "Someone??";
    private static bool PlayerIsDown(int slot) => !GetPlayerFrom(slot)?.Alive ?? false;
    private static PlayerAgent? GetPlayerFrom(int slot) => PlayerManager.Current.GetPlayerAgentInSlot(slot);
}
