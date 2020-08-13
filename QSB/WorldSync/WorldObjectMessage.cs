using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.WorldSync
{
    public class WorldObjectMessage : PlayerMessage
    {
        public string UniqueName { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            UniqueName = reader.ReadString();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(UniqueName);
        }
    }
}
