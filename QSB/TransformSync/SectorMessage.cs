using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.TransformSync
{
    public class SectorMessage : QSBMessage
    {
        public override MessageType MessageType => MessageType.Sector;
        public Sector.Name SectorName => (Sector.Name)SectorId;

        public int SectorId;
        public uint SenderId;

        public override void Deserialize(NetworkReader reader)
        {
            SectorId = reader.ReadInt32();
            SenderId = reader.ReadPackedUInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(SectorId);
            writer.Write(SenderId);
        }
    }
}