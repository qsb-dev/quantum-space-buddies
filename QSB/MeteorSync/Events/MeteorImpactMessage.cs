using QSB.WorldSync.Events;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.MeteorSync.Events
{
	public class MeteorImpactMessage : WorldObjectMessage
	{
		public Vector3 Position;
		public Vector3 RelativeVelocity;
		public float Damage;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Position = reader.ReadVector3();
			RelativeVelocity = reader.ReadVector3();
			Damage = reader.ReadSingle();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Position);
			writer.Write(RelativeVelocity);
			writer.Write(Damage);
		}
	}
}
