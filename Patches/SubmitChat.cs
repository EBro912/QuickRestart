using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace QuickRestart.Patches
{
    [HarmonyPatch(typeof(HUDManager), "SubmitChat_performed")]
    public class SubmitChat
    {
        // TODO: make this not ugly
        private static bool Prefix(HUDManager __instance, ref InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return true;
            }
            if (string.IsNullOrEmpty(__instance.chatTextField.text))
            {
                return true;
            }
            PlayerControllerB local = GameNetworkManager.Instance.localPlayerController;
            if (local == null)
            {
                return true;
            }
            StartOfRound manager = local.playersManager;
            if (manager == null)
            {
                return true;
            }
            string text = __instance.chatTextField.text;
            if (Plugin.verifying)
            {
                if (text == "CONFIRM")
                {
                    ResetTextbox(__instance, local);
                    if (!local.isInHangarShipRoom || !manager.inShipPhase || manager.travellingToNewLevel)
                    {
                        Plugin.SendChatMessage("Cannot restart, ship must be in orbit.");
                        return false;
                    }
                    Plugin.AcceptRestart(manager);
                    return false;
                }
                if (text == "DENY")
                {
                    ResetTextbox(__instance, local);
                    Plugin.DeclineRestart();
                    return false;
                }
                return true;
            }
            if (text == "/restart")
            {
                ResetTextbox(__instance, local);
                if (!GameNetworkManager.Instance.isHostingGame)
                {
                    Plugin.SendChatMessage("Only the host can restart.");
                    return false;
                }
                if (!local.isInHangarShipRoom || !manager.inShipPhase || manager.travellingToNewLevel)
                {
                    Plugin.SendChatMessage("Cannot restart, ship must be in orbit.");
                    return false;
                }
                Plugin.ConfirmRestart();
                return false;
            }
            return true;
        }

        private static void ResetTextbox(HUDManager manager, PlayerControllerB local)
        {
            local.isTypingChat = false;
            manager.chatTextField.text = "";
            EventSystem.current.SetSelectedGameObject(null);
            manager.PingHUDElement(manager.Chat, 2f, 1f, 0.2f);
            manager.typingIndicator.enabled = false;
        }
    }
}
