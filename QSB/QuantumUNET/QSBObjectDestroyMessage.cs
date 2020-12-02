using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	class QSBObjectDestroyMessage : MessageBase
	{
		public override void Deserialize(NetworkReader reader)
		{
			netId = reader.ReadNetworkId();
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(netId);
		}

		public NetworkInstanceId netId;
	}
}
