using System;
using System.Linq;
using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class StateRequestMessage : PlayerMessage
    {
        public override MessageType MessageType => MessageType.FullStateRequest;
        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
        }
    }
}
