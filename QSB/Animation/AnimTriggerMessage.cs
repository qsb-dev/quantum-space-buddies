using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Animation
{
    public class AnimTriggerMessage : QSBMessage
    {
        public short TriggerId;
        public uint SenderId;
        public float Value;

        public override void Deserialize(NetworkReader reader)
        {
            Value = reader.ReadSingle();
            TriggerId = reader.ReadInt16();
            SenderId = reader.ReadPackedUInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Value);
            writer.Write(TriggerId);
            writer.Write(SenderId);
        }
    }
}
