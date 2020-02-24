using OWML.ModHelper.Events;
using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.TimeSync
{
    public class WakeUpSync : NetworkBehaviour
    {
        private MessageHandler<WakeUpMessage> _wakeUpHandler;

        private void Start()
        {
            DebugLog.Screen("Start WakeUpSync");
            if (!isLocalPlayer)
            {
                return;
            }
            GlobalMessenger.AddListener("WakeUp", OnWakeUp);
            _wakeUpHandler = new MessageHandler<WakeUpMessage>();
            _wakeUpHandler.OnClientReceiveMessage += OnClientReceiveMessage;
        }

        private void OnWakeUp()
        {
            if (!isServer)
            {
                return;
            }
            DebugLog.Screen("Sending wakeup to all my friends");
            _wakeUpHandler.SendToAll(new WakeUpMessage());
        }

        private void OnClientReceiveMessage(WakeUpMessage message)
        {
            if (isServer)
            {
                return;
            }

            // I copied all of this from my AutoResume mod, since that already wakes up the player instantly.
            // There must be a simpler way to do this though, I just couldn't find it.

            // Skip wake up animation.
            var cameraEffectController = FindObjectOfType<PlayerCameraEffectController>();
            cameraEffectController.OpenEyes(0, true);
            cameraEffectController.SetValue("_wakeLength", 0f);
            cameraEffectController.SetValue("_waitForWakeInput", false);

            // Skip wake up prompt.
            LateInitializerManager.pauseOnInitialization = false;
            Locator.GetPauseCommandListener().RemovePauseCommandLock();
            Locator.GetPromptManager().RemoveScreenPrompt(cameraEffectController.GetValue<ScreenPrompt>("_wakePrompt"));
            OWTime.Unpause(OWTime.PauseType.Sleeping);
            cameraEffectController.Invoke("WakeUp");

            // Enable all inputs immediately.
            OWInput.ChangeInputMode(InputMode.Character);
            typeof(OWInput).SetValue("_inputFadeFraction", 0f);
            GlobalMessenger.FireEvent("TakeFirstFlashbackSnapshot");
        }

    }
}
