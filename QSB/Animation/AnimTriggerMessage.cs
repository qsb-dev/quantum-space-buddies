using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Animation
{
    public class AnimTriggerMessage : QSBMessage
    {
        public override MessageType MessageType => MessageType.AnimTrigger;

        public short TriggerId;
        public uint SenderId;

        public override void Deserialize(NetworkReader reader)
        {
            TriggerId = reader.ReadInt16();
            SenderId = reader.ReadPackedUInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(TriggerId);
            writer.Write(SenderId);
        }
    }
}
