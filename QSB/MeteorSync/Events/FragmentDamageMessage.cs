using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;
using QuantumUNET.Transport;

namespace QSB.MeteorSync.Events
{
	public class FragmentDamageMessage : QSBWorldObjectMessage<QSBFragment>
	{
		private float Damage;

		public FragmentDamageMessage(float damage) => Damage = damage;

		public FragmentDamageMessage() { }

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Damage);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Damage = reader.ReadSingle();
		}

		public override void OnReceiveRemote() => WorldObject.AddDamage(Damage);
	}
}