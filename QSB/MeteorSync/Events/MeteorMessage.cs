using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.MeteorSync.Events {
    public class MeteorMessage : WorldObjectMessage {
        public float num, launchSpeed, damage;

        public override void Deserialize(QNetworkReader reader) {
            base.Deserialize(reader);
            num = reader.ReadSingle();
            launchSpeed = reader.ReadSingle();
            damage = reader.ReadSingle();
        }

        public override void Serialize(QNetworkWriter writer) {
            base.Serialize(writer);
            writer.Write(num);
            writer.Write(launchSpeed);
            writer.Write(damage);
        }

        public override string ToString() => $"{nameof(num)}: {num}, {nameof(launchSpeed)}: {launchSpeed}, {nameof(damage)}: {damage}";
    }
}
