using OWML.ModHelper.Events;
using System;
using UnityEngine.Networking;

namespace QSB {
    class WakeUpSync: MessageHandler {
        protected override MessageType type => MessageType.WakeUp;
        public static bool isServer;

        void Start () {
            DebugLog.Screen("Start WakeUpSync");
            GlobalMessenger.AddListener("WakeUp", OnWakeUp);
        }

        void OnWakeUp () {
            DebugLog.Screen("Sending wakeup to all my friends");
            if (isServer) {
                var message = new WakeUpMessage();
                NetworkServer.SendToAll((short) MessageType.WakeUp, message);
            }
        }

        protected override void OnClientReceiveMessage (NetworkMessage netMsg) {
            if (isServer) {
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
