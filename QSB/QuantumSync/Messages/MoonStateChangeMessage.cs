using QSB.Messaging;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.QuantumSync.Messages
{
	public class MoonStateChangeMessage : PlayerMessage
	{
		public int StateIndex { get; set; }
		public Vector3 OnUnitSphere { get; set; }
		public int OrbitAngle { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			StateIndex = reader.ReadInt32();
			OnUnitSphere = reader.ReadVector3();
			OrbitAngle = reader.ReadInt32();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(StateIndex);
			writer.Write(OnUnitSphere);
			writer.Write(OrbitAngle);
		}
	}
}
