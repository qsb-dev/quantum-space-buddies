using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.WorldSync
{
    public class WorldObjectMessage : PlayerMessage
    {
        public string ObjectName { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            ObjectName = reader.ReadString();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(ObjectName);
        }
    }
}
