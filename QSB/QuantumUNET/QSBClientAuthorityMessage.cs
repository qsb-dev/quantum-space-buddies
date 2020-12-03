using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	internal class QSBClientAuthorityMessage : MessageBase
	{
		public NetworkInstanceId netId;
		public bool authority;

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
	}
}