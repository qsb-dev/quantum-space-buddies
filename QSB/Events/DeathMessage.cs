using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class DeathMessage : NameMessage
    {
        public override MessageType MessageType => MessageType.Death;

        public short DeathId { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            DeathId = reader.ReadInt16();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(DeathId);
        }
    }
}
