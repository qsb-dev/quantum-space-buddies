using QSB.WorldSync.Events;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.QuantumSync.Events
{
	public class SocketStateChangeMessage : WorldObjectMessage
	{
		public int SocketId { get; set; }
		public Quaternion LocalRotation { get; set; }

		public override void Deserialize(QSBNetworkReader reader)
		{
			base.Deserialize(reader);
			SocketId = reader.ReadInt32();
			LocalRotation = reader.ReadQuaternion();
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(SocketId);
			writer.Write(LocalRotation);
		}
	}
}
