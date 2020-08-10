using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Messaging
{
    public class AnimTriggerMessage : PlayerMessage
    {
        public short TriggerId;
        public float Value;

        public override void Deserialize(NetworkReader reader)
        {
            Value = reader.ReadSingle();
            TriggerId = reader.ReadInt16();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Value);
            writer.Write(TriggerId);
        }
    }
}
