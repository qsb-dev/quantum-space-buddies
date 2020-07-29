using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class EventMessage : PlayerMessage
    {
        public override MessageType MessageType => MessageType.Event;

        public int EventType { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            EventType = reader.ReadInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(EventType);
        }
    }
}
