using UnityEngine.Networking;

namespace QSB.Messaging
{
    public class FullStateMessage : PlayerMessage
    {
        public string PlayerName { get; set; }
        public bool IsReady { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            PlayerName = reader.ReadString();
            IsReady = reader.ReadBoolean();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(PlayerName);
            writer.Write(IsReady);
        }
    }
}
