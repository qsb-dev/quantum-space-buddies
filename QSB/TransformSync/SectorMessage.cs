using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.TransformSync
{
    public class SectorMessage : QSBMessage
    {
        public override MessageType MessageType => MessageType.Sector;

        public int SectorName;
        public uint SenderId;

        public override void Deserialize(NetworkReader reader)
        {
            SectorName = reader.ReadInt32();
            SenderId = reader.ReadPackedUInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(SectorName);
            writer.Write(SenderId);
        }
    }
}