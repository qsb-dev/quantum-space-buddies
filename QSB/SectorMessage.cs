using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB
{
    public class SectorMessage : QSBMessage
    {
        public override short MessageType => MsgType.Highest + 1;

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