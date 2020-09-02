using UnityEngine.Networking;

namespace QSB.Messaging
{
    public class PlayerMessage : MessageBase
    {
        public NetworkInstanceId FromId { get; set; }
        public NetworkInstanceId AboutId { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            FromId = reader.ReadNetworkId();
            AboutId = reader.ReadNetworkId();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(FromId);
            writer.Write(AboutId);
        }
    }
}
