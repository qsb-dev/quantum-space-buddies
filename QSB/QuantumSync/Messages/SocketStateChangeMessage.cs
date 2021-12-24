using QSB.WorldSync.Events;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.QuantumSync.Messages
{
	public class SocketStateChangeMessage : WorldObjectMessage
	{
		public int SocketId { get; set; }
		public Quaternion LocalRotation { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			SocketId = reader.ReadInt32();
			LocalRotation = reader.ReadQuaternion();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(SocketId);
			writer.Write(LocalRotation);
		}
	}
}
