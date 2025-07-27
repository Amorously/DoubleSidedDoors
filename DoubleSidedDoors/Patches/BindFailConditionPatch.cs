using HarmonyLib;
using LevelGeneration;
using Player;
using static DSD.Module.DoorConfigManager;

namespace DSD.Patches;

[HarmonyPatch(typeof(WardenObjectiveManager), nameof(WardenObjectiveManager.CheckExpeditionFailed))]
internal static class BindFailConditionPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.HigherThanNormal)]
    [HarmonyWrapSafe]
    private static void CheckExpeditionFailed(ref bool __result)
    {
        if (__result) return;

        foreach (var custom in Current)
        {
            foreach (var bindSet in custom.BindToPlayer)
            {
                if (AllPlayersAreDownInSet(bindSet, out var deadPlayerNames) && custom.Identifier.SecDoor.LastStatus != eDoorStatus.Open)
                {
                    DSDLogger.Warn($"{string.Join(" + ", deadPlayerNames)} were downed while bound to unopened door -> attempting end run!");
                    __result = true;
                    return;
                }
            }
        }
    }

    private static bool AllPlayersAreDownInSet(HashSet<int> set, out IEnumerable<string> deadPlayerNames)
    {
        deadPlayerNames = set.Where(PlayerIsDown).Select(DeadPlayerNames);

        var validSet = set.Select(GetPlayerInSlot).Where(player => player != null);
        if (!validSet.Any())
        {
            return set.All(PlayerIsDown);
        }

        return validSet.All(player => !player!.Alive);

        string DeadPlayerNames(int slot) => GetPlayerInSlot(slot)?.PlayerName ?? "Someone??";
        bool PlayerIsDown(int slot) => !GetPlayerInSlot(slot)?.Alive ?? false;
        PlayerAgent? GetPlayerInSlot(int slot) => PlayerManager.Current.GetPlayerAgentInSlot(slot);
    }
}
