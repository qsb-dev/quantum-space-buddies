using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Swag
{
    public class HelloMessage : QSBMessage
    {
        public override MessageType MessageType => MessageType.Hello;

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
