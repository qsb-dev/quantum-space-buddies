using UnityEngine.Networking;

namespace QSB.Messaging
{
    public class WakeUpMessage : MessageBase
    {
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