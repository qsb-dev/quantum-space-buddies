using UnityEngine.Networking;

namespace QSB {
    public class WakeUpMessage: MessageBase {
        bool wakeUp = true;

        public override void Deserialize (NetworkReader reader) {
            wakeUp = reader.ReadBoolean();
        }

        public override void Serialize (NetworkWriter writer) {
            writer.Write(wakeUp);
        }
    }
}