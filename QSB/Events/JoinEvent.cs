using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class JoinEvent : QSBMessage
    {
        public override MessageType MessageType => MessageType.Join;

        public string PlayerName { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            PlayerName = reader.ReadString();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(PlayerName);
        }
    }
}
