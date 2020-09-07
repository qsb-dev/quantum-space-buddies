using UnityEngine.Networking;

namespace QSB.WorldSync
{
    public class BoolWorldObjectMessage : WorldObjectMessage
    {
        public bool State { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            State = reader.ReadBoolean();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(State);
        }
    }
}
