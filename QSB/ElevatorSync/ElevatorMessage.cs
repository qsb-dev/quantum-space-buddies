using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.ElevatorSync
{
    public class ElevatorMessage : PlayerMessage
    {
        public ElevatorDirection Direction { get; set; }
        public string ElevatorName { get; set; }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            Direction = (ElevatorDirection)reader.ReadInt32();
            ElevatorName = reader.ReadString();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)Direction);
            writer.Write(ElevatorName);
        }
    }
}
