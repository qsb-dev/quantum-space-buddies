using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	internal class QSBClientAuthorityMessage : MessageBase
	{
		public override void Deserialize(NetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			authority = reader.ReadBoolean();
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(netId);
			writer.Write(authority);
		}

		public NetworkInstanceId netId;

		public bool authority;
	}
}