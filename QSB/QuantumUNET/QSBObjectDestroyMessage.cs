using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	internal class QSBObjectDestroyMessage : MessageBase
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