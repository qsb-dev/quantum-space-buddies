using QSB.Messaging;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.StatueSync.Messages
{
	public class StartStatueMessage : PlayerMessage
	{
		public Vector3 PlayerPosition { get; set; }
		public Quaternion PlayerRotation { get; set; }
		public float CameraDegrees { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			PlayerPosition = reader.ReadVector3();
			PlayerRotation = reader.ReadQuaternion();
			CameraDegrees = reader.ReadSingle();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerPosition);
			writer.Write(PlayerRotation);
			writer.Write(CameraDegrees);
		}
	}
}
