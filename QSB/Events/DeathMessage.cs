using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class DeathMessage : QSBMessage
    {
        public override MessageType MessageType => MessageType.Death;

        public string PlayerName { get; set; }
        public uint SenderId { get; set; }
        public short DeathId { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            PlayerName = reader.ReadString();
            DeathId = reader.ReadInt16();
            SenderId = reader.ReadUInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(PlayerName);
            writer.Write(DeathId);
            writer.Write(SenderId);
        }
    }
}
