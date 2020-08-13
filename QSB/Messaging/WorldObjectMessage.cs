using QSB.WorldSync;
using UnityEngine.Networking;

namespace QSB.Messaging
{
    public class WorldObjectMessage : PlayerMessage
    {
        public SyncObjects ObjectType { get; set; }
        public int ObjectID { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            ObjectType = (SyncObjects)reader.ReadInt32();
            ObjectID = reader.ReadInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)ObjectType);
            writer.Write(ObjectID);
        }
    }
}
