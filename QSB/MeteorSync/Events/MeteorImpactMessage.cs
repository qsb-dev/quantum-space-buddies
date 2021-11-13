using QSB.WorldSync.Events;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.MeteorSync.Events
{
	public class MeteorImpactMessage : WorldObjectMessage
	{
		public Vector3 Pos;
		public Quaternion Rot;
		public float Damage;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Pos = reader.ReadVector3();
			Rot = reader.ReadQuaternion();
			Damage = reader.ReadSingle();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Pos);
			writer.Write(Rot);
			writer.Write(Damage);
		}
	}
}
