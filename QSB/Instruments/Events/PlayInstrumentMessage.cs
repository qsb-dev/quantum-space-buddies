using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Instruments.Events
{
    public class PlayInstrumentMessage : PlayerMessage
    {
        public InstrumentType Type;
        public bool State;

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            Type = (InstrumentType)reader.ReadInt32();
            State = reader.ReadBoolean();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)Type);
            writer.Write(State);
        }
    }
}
