using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Animation
{
    public class AnimTriggerMessage : QSBMessage
    {
        public override MessageType MessageType => MessageType.AnimTrigger;

        public string TriggerName;
        public uint SenderId;

        public override void Deserialize(NetworkReader reader)
        {
            TriggerName = reader.ReadString();
            SenderId = reader.ReadPackedUInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(TriggerName);
            writer.Write(SenderId);
        }

    }
}
