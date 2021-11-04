using QSB.WorldSync.Events;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.AnglerFish.Events {
    public class AnglerResyncMessage : WorldObjectMessage {
        public Vector3 pos;
        public Quaternion rot;

        public override void Deserialize(QNetworkReader reader) {
            base.Deserialize(reader);
            pos = reader.ReadVector3();
            rot = reader.ReadQuaternion();
        }

        public override void Serialize(QNetworkWriter writer) {
            base.Serialize(writer);
            writer.Write(pos);
            writer.Write(rot);
        }
    }
}
