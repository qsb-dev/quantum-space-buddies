using QuantumUNET.Transport;
using UnityEngine.Networking;

namespace QuantumUNET.Messages
{
	internal class QClientAuthorityMessage : QMessageBase
	{
		public NetworkInstanceId netId;
		public bool authority;

		public override void Deserialize(QNetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			authority = reader.ReadBoolean();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write(netId);
			writer.Write(authority);
		}
	}
}