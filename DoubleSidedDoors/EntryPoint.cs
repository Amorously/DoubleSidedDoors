using AmorLib.Dependencies;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using DSD.Module;
using GTFO.API;
using HarmonyLib;
using System.Runtime.CompilerServices;

namespace DSD;

/* TODO:
 * Flipped blood/scream doors
 * Try setup for ProgressionPuzzleToEnter types
 * Doors interactable on both sides
*/

[BepInPlugin("Amor.DoubleSidedDoors", "DoubleSidedDoors", "0.8.0")]
[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("com.dak.MTFO", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("Amor.AmorLib", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(PData_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInIncompatibility("randomuserhi.DoubleSidedDoors")]
internal sealed class EntryPoint : BasePlugin
{
    public override void Load()
    {
        new Harmony("Amor.DoubleSidedDoors").PatchAll();
        AssetAPI.OnStartupAssetsLoaded += OnStartupAssetsLoaded;
        DSDLogger.Info("DSD is done loading!");
    }

    private void OnStartupAssetsLoaded()
    {
        RuntimeHelpers.RunClassConstructor(typeof(DoorConfigManager).TypeHandle);
    }
}