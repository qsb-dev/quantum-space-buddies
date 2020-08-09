using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Messaging
{
    public class WakeUpMessage : PlayerMessage
    {
        public float ServerTime { get; set; }
        public int LoopCount { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            ServerTime = reader.ReadSingle();
            LoopCount = reader.ReadInt16();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(ServerTime);
            writer.Write(LoopCount);
        }

    }
}