using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	internal class QClientAuthorityMessage : QMessageBase
	{
		public QNetworkInstanceId netId;
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