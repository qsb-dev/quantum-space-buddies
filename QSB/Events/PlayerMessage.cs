using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Events
{
    public abstract class PlayerMessage : QSBMessage
    {
        public uint SenderId { get; set; }
        
        public override void Deserialize(NetworkReader reader)
        {
            SenderId = reader.ReadUInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(SenderId);
        }
    }
}
