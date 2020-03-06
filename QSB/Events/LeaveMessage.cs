using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class LeaveMessage : NameMessage
    {
        public override MessageType MessageType => MessageType.Leave;

        public uint ShipId { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            ShipId = reader.ReadUInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(ShipId);
        }
    }
}
