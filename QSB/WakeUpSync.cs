using OWML.ModHelper.Events;
using System;
using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB
{
    public class WakeUpSync : MessageHandler
    {
        public static bool IsServer;

        protected override MessageType Type => MessageType.WakeUp;

        private void Start()
        {
            DebugLog.Screen("Start WakeUpSync");
            GlobalMessenger.AddListener("WakeUp", OnWakeUp);
        }

        private void OnWakeUp()
        {
            DebugLog.Screen("Sending wakeup to all my friends");
            if (IsServer)
            {
                var message = new WakeUpMessage();
                NetworkServer.SendToAll((short)MessageType.WakeUp, message);
            }
        }

        protected override void OnClientReceiveMessage(NetworkMessage netMsg)
        {
            if (IsServer)
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

        protected override void OnServerReceiveMessage(NetworkMessage netMsg)
        {
            throw new NotImplementedException();
        }

    }
}
