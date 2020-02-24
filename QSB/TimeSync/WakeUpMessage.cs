using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.TimeSync
{
    public class WakeUpMessage : QSBMessage
    {
        public override MessageType MessageType => MessageType.WakeUp;

        public float ServerTime { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            ServerTime = reader.ReadSingle();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(ServerTime);
        }

    }
}