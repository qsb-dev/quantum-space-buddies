using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Messaging
{
    public class SectorMessage : PlayerMessage
    {
        public int SectorId;

        public override void Deserialize(NetworkReader reader)
        {
            SectorId = reader.ReadInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(SectorId);
        }
    }
}