using QSB.WorldSync.Events;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.ShipSync.Messages
{
	public class ImpactDataMessage : WorldObjectMessage
	{
		public Vector3 Point { get; set; }
		public Vector3 Normal { get; set; }
		public Vector3 Velocity { get; set; }
		public float Speed { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Point = reader.ReadVector3();
			Normal = reader.ReadVector3();
			Velocity = reader.ReadVector3();
			Speed = reader.ReadSingle();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Point);
			writer.Write(Normal);
			writer.Write(Velocity);
			writer.Write(Speed);
		}
	}
}
