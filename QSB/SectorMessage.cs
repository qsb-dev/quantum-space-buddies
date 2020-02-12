using UnityEngine.Networking;

namespace QSB {
    public class SectorMessage: MessageBase {
        public static short Type = MsgType.Highest + 1;
        public int sectorId;
        public uint senderId;

        public override void Deserialize (NetworkReader reader) {
            sectorId = reader.ReadInt32();
            senderId = reader.ReadPackedUInt32();
        }

        public override void Serialize (NetworkWriter writer) {
            writer.Write(sectorId);
            writer.Write(senderId);
        }
    }
}