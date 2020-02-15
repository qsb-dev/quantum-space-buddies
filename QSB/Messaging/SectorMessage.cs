using UnityEngine.Networking;

namespace QSB.Messaging
{
    public class SectorMessage : MessageBase
    {
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