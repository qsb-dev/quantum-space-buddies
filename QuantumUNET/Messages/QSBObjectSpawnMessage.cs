using UnityEngine;

namespace QuantumUNET.Messages
{
	internal class QSBObjectSpawnMessage : QSBMessageBase
	{
		public QSBNetworkInstanceId NetId;
		public QSBNetworkHash128 assetId;
		public Vector3 Position;
		public byte[] Payload;
		public Quaternion Rotation;

		public override void Deserialize(QSBNetworkReader reader)
		{
			NetId = reader.ReadNetworkId();
			assetId = reader.ReadNetworkHash128();
			Position = reader.ReadVector3();
			Payload = reader.ReadBytesAndSize();
			if (reader.Length - reader.Position >= 16U)
			{
				Rotation = reader.ReadQuaternion();
			}
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.Write(NetId);
			writer.Write(assetId);
			writer.Write(Position);
			writer.WriteBytesFull(Payload);
			writer.Write(Rotation);
		}
	}
}