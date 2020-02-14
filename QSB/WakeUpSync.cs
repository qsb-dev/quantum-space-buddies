using OWML.ModHelper.Events;
using System;
using UnityEngine.Networking;

namespace QSB {
    class WakeUpSync: MessageHandler {
        protected override short type => MessageType.WakeUp;

        void Start () {
            DebugLog.Screen("Start WakeUpSync");
            GlobalMessenger.AddListener("WakeUp", OnWakeUp);
        }

        void OnWakeUp () {
            DebugLog.Screen("Sending wakeup to all my friends");
            var message = new WakeUpMessage();
            NetworkServer.SendToAll(MessageType.WakeUp, message);
        }

        protected override void OnClientReceiveMessage (NetworkMessage netMsg) {
            DebugLog.Screen("client received wake up message");
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
            //cameraEffectController.Invoke("WakeUp");

            // Enable all inputs immedeately.
            OWInput.ChangeInputMode(InputMode.Character);
            typeof(OWInput).SetValue("_inputFadeFraction", 0f);
            GlobalMessenger.FireEvent("TakeFirstFlashbackSnapshot");
        }

        protected override void OnServerReceiveMessage (NetworkMessage netMsg) {
            throw new NotImplementedException();
        }
    }
}
