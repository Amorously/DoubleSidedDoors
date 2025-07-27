using HarmonyLib;
using LevelGeneration;
using UnityEngine;
using static DSD.Module.DoorConfigManager;

namespace DSD.Patches;

[HarmonyPatch(typeof(LG_SecurityDoor), nameof(LG_SecurityDoor.Setup))]
internal static class SecurityDoorSetupPatch
{
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    private static void SecDoor_Setup(LG_SecurityDoor __instance, LG_Gate gate)
    {
        if (__instance.m_securityDoorType != eSecurityDoorType.Security || __instance.DoorType != eLG_DoorType.Security) return;

        var custom = Current.FirstOrDefault(cfg => cfg.GlobalZoneIndex.Equals(GetGlobalIndexFrom(gate)));
        if (custom == null) return;

        Transform? crossing = __instance.transform.Find("crossing");
        if (crossing == null) return;
        else if (custom.Flipped) // flip door
        {
            crossing.localRotation *= Quaternion.Euler(0, 180, 0);
        }

        GameObject? rearInteractionMessage = null;
        if (custom.DoubleHandle) // add rear handle
        {
            Transform? capBack = __instance.m_doorBladeCuller.transform.Find("securityDoor_8x4_tech/bottomDoor/g_securityDoor_bottomDoor_capback") ??
                            __instance.m_doorBladeCuller.transform.Find("securityDoor_4x4_tech (1)/rightDoor/g_securityDoor_bottomDoor_capback001");
            if (capBack == null) return;
            capBack.gameObject.SetActive(false);

            Transform? handle = __instance.m_doorBladeCuller.transform.Find("securityDoor_8x4_tech/bottomDoor/InteractionInterface") ??
                               __instance.m_doorBladeCuller.transform.Find("securityDoor_4x4_tech (1)/rightDoor/InteractionInterface");
            if (handle == null) return;
            bool size4x4 = __instance.m_doorBladeCuller.transform.Find("securityDoor_4x4_tech (1)") != null;
            GameObject backHandle = UnityEngine.Object.Instantiate(handle.gameObject, handle.parent);
            backHandle.transform.localRotation = handle.localRotation * Quaternion.Euler(180, 180, 0);
            backHandle.transform.localPosition = handle.localPosition + (size4x4 ? new Vector3(0, 0.28f, 0) : new Vector3(0, 0, -0.25f));

            Transform? interactMessage = __instance.transform.Find("crossing/Interaction_Message");
            if (interactMessage == null) return;
            rearInteractionMessage = UnityEngine.Object.Instantiate(interactMessage.gameObject, interactMessage.parent);
            rearInteractionMessage.transform.position = backHandle.transform.position;
            Interact_MessageOnScreen message = rearInteractionMessage.GetComponent<Interact_MessageOnScreen>();
            message.MessageType = eMessageOnScreenType.InteractionPrompt;
            message.m_message = custom.RearHandleText;

            rearInteractionMessage.SetActive(true);
            message.SetActive(true);
        }

        custom.Identifier.Initialize(__instance, gate, rearInteractionMessage);
        DSDLogger.Verbose($"SEC_DOOR_{__instance.m_serialNumber} {custom} setup as {custom.Type}\n");   
    }
}
