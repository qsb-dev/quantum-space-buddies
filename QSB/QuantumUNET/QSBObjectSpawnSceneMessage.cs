using UnityEngine;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	internal class QSBObjectSpawnSceneMessage : MessageBase
	{
		public NetworkInstanceId NetId;
		public NetworkSceneId SceneId;
		public Vector3 Position;
		public byte[] Payload;

		public override void Deserialize(NetworkReader reader)
		{
			NetId = reader.ReadNetworkId();
			SceneId = reader.ReadSceneId();
			Position = reader.ReadVector3();
			Payload = reader.ReadBytesAndSize();
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(NetId);
			writer.Write(SceneId);
			writer.Write(Position);
			writer.WriteBytesFull(Payload);
		}
	}
}