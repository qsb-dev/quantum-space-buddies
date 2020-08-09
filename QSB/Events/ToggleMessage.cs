using UnityEngine.Networking;

namespace QSB.Events
{
    public class ToggleMessage : PlayerMessage
    {
        public bool On { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            On = reader.ReadBoolean();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(On);
        }
    }
}