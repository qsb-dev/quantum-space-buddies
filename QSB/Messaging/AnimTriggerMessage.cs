using UnityEngine.Networking;

namespace QSB.Messaging
{
    public class AnimTriggerMessage : MessageBase
    {
        public short TriggerId;
        public uint SenderId;
        public float Value;

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            Value = reader.ReadSingle();
            TriggerId = reader.ReadInt16();
            SenderId = reader.ReadPackedUInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Value);
            writer.Write(TriggerId);
            writer.Write(SenderId);
        }
    }
}
