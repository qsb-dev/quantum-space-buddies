using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	public class QEmptyMessage : QMessageBase
	{
		public override void Serialize(QNetworkWriter writer) { }
		public override void Deserialize(QNetworkReader reader) { }
	}
}