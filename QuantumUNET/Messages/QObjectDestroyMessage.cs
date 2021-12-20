using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	internal class QObjectDestroyMessage : QMessageBase
	{
		public QNetworkInstanceId NetId;

		public override void Serialize(QNetworkWriter writer) => writer.Write(NetId);

		public override void Deserialize(QNetworkReader reader) => NetId = reader.ReadNetworkId();
	}
}