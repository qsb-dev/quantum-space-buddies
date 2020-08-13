using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.TransformSync
{
    public class SectorMessage : PlayerMessage
    {
        public Sector.Name SectorId;
        public string SectorName;

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            SectorId = (Sector.Name)reader.ReadInt32();
            SectorName = reader.ReadString();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)SectorId);
            writer.Write(SectorName);
        }
    }
}