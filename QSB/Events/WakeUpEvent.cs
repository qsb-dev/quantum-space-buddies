using QSB.Messaging;
using QSB.TimeSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.Events
{
    class WakeUpEvent : QSBEvent<WakeUpMessage>
    {
        public override MessageType Type => MessageType.WakeUp;

        public override void SetupListener()
        {
            GlobalMessenger<float, int>.AddListener("QSBServerTime", (time, count) => SendEvent(
                new WakeUpMessage { 
                    SenderId = PlayerRegistry.LocalPlayer.NetId,
                    ServerTime = time, 
                    LoopCount = count 
                }));
        }

        public override void OnReceive(WakeUpMessage message)
        {
            WakeUpSync.LocalInstance.OnClientReceiveMessage(message);
        }
    }
}
