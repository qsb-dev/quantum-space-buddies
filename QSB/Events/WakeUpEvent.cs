using QSB.Messaging;
using QSB.TimeSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.Events
{
    class WakeUpEvent : QSBEvent<ServerTimeMessage>
    {
        public override MessageType Type => MessageType.WakeUp;

        public override void SetupListener()
        {
            GlobalMessenger<float, int>.AddListener("QSBServerTime", (time, count) => SendEvent(
                new ServerTimeMessage { 
                    SenderId = PlayerRegistry.LocalPlayer.NetId,
                    ServerTime = time, 
                    LoopCount = count 
                }));
        }

        public override void OnReceive(ServerTimeMessage message)
        {
            WakeUpSync.LocalInstance.OnClientReceiveMessage(message);
        }
    }
}
