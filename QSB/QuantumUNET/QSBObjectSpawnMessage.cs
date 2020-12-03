using UnityEngine;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	internal class QSBObjectSpawnMessage : MessageBase
	{
		public NetworkInstanceId NetId;
		public NetworkHash128 assetId;
		public Vector3 Position;
		public byte[] Payload;
		public Quaternion Rotation;

		public override void Deserialize(NetworkReader reader)
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

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(NetId);
			writer.Write(assetId);
			writer.Write(Position);
			writer.WriteBytesFull(Payload);
			writer.Write(Rotation);
		}
	}
}