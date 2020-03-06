using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Events
{
    public abstract class NameMessage : QSBMessage
    {
        public uint SenderId { get; set; }
        public string PlayerName { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            PlayerName = reader.ReadString();
            SenderId = reader.ReadUInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(PlayerName);
            writer.Write(SenderId);
        }
    }
}
