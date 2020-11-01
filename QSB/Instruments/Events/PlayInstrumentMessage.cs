using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Instruments.Events
{
    public class PlayInstrumentMessage : PlayerMessage
    {
        public InstrumentType Type;

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            Type = (InstrumentType)reader.ReadInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)Type);
        }
    }
}
