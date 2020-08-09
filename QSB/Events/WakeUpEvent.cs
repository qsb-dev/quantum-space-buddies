using QSB.Messaging;
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
                    ServerTime = time, 
                    LoopCount = count 
                }));
        }
    }
}
