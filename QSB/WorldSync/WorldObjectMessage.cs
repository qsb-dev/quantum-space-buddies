using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.WorldSync
{
    public class WorldObjectMessage : PlayerMessage
    {
        public int Id { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            Id = reader.ReadInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Id);
        }
    }
}
