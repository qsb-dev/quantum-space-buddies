using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.TimeSync
{
    public class WakeUpMessage : QSBMessage
    {
        public override MessageType MessageType => MessageType.WakeUp;

        private bool _wakeUp = true;

        public override void Deserialize(NetworkReader reader)
        {
            _wakeUp = reader.ReadBoolean();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(_wakeUp);
        }

    }
}