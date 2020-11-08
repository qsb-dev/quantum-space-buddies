using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Animation.Events
{
    public class ChangeAnimTypeMessage : PlayerMessage
    {
        public AnimationType Type;

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            Type = (AnimationType)reader.ReadInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)Type);
        }
    }
}