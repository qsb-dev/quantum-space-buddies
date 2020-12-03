using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	internal class QSBObjectDestroyMessage : MessageBase
	{
		public NetworkInstanceId NetId;

		public override void Deserialize(NetworkReader reader)
		{
			NetId = reader.ReadNetworkId();
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(NetId);
		}
	}
}