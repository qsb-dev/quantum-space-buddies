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
            GlobalMessenger.FireEvent("WakeUp");
        }

        protected override void OnServerReceiveMessage (NetworkMessage netMsg) {
            throw new NotImplementedException();
        }

        void Update () {
            if (Input.GetKeyDown(UnityEngine.KeyCode.KeypadPlus)) {
                OnWakeUp();
            }
        }
    }
}
