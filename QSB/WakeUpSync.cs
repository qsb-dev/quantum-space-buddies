using System;
using UnityEngine.Networking;

namespace QSB {
    class WakeUpSync: MessageHandler {
        protected override short type => MessageType.WakeUp;

        protected override void OnClientReceiveMessage (NetworkMessage netMsg) {
            GlobalMessenger.FireEvent("WakeUp");
        }

        protected override void OnServerReceiveMessage (NetworkMessage netMsg) {
            throw new NotImplementedException();
        }
    }
}
