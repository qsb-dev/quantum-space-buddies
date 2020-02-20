using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB
{
    public class WakeUpMessage : QSBMessage
    {
        public override short MessageType => MsgType.Highest + 2;

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