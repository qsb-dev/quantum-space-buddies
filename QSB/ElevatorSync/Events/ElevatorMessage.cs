using QSB.WorldSync;
using UnityEngine.Networking;

namespace QSB.ElevatorSync.Events
{
    public class ElevatorMessage : WorldObjectMessage
    {
        public ElevatorDirection Direction { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            Direction = (ElevatorDirection)reader.ReadInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)Direction);
        }
    }
}
