using QuantumUNET.Transport;
using UnityEngine;

namespace QuantumUNET.Messages
{
	internal class QObjectSpawnMessage : QMessageBase
	{
		public NetworkInstanceId NetId;
		public int assetId;
		public Vector3 Position;
		public byte[] Payload;
		public Quaternion Rotation;

		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write(NetId);
			writer.Write(assetId);
			writer.Write(Position);
			writer.WriteBytesFull(Payload);
			writer.Write(Rotation);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			NetId = reader.ReadNetworkId();
			assetId = reader.ReadInt32();
			Position = reader.ReadVector3();
			Payload = reader.ReadBytesAndSize();
			if (reader.Length - reader.Position >= 16U)
			{
				Rotation = reader.ReadQuaternion();
			}
		}
	}
}