using DoubleSidedDoors.Module;
using DoubleSidedDoors.Utils;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace DoubleSidedDoors;

/* TODO:
 * Flipped blood/scream doors
 * Try setup for ProgressionPuzzleToEnter types
 * Doors interactable on both sides
 */

[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency(MTFO.MTFO.GUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(PartialDataUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
public class EntryPoint : BasePlugin
{
    public const string GUID = "Amor.DoubleSidedDoors";
    public const string NAME = "DoubleSidedDoors";
    public const string VERSION = "0.7.2";

    public override void Load()
    {
        if (IL2CPPChainloader.Instance.Plugins.ContainsKey("randomuserhi.DoubleSidedDoors"))
        {
            DSDLogger.Error("OG DoubleSidedDoors present!\nTo prevent potential problems: Please either create a fresh profile or find and delete the old .dll");
            return;
        }

        DSDLogger.Log("Plugin is loaded!");
        new Harmony(GUID).PatchAll();
        DSDLogger.Log("Debug is " + (ConfigManager.Debug ? "Enabled" : "Disabled"));
        
        DoorBehavior.Init();
    }
}