using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	class QSBObjectSpawnSceneMessage : MessageBase
	{
		public override void Deserialize(NetworkReader reader)
		{
			this.netId = reader.ReadNetworkId();
			this.sceneId = reader.ReadSceneId();
			this.position = reader.ReadVector3();
			this.payload = reader.ReadBytesAndSize();
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(this.netId);
			writer.Write(this.sceneId);
			writer.Write(this.position);
			writer.WriteBytesFull(this.payload);
		}

		public NetworkInstanceId netId;

		public NetworkSceneId sceneId;

		public Vector3 position;

		public byte[] payload;
	}
}
