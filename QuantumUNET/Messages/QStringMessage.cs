using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	public class QStringMessage : QMessageBase
	{
		public string value;

		public QStringMessage(string v)
		{
			value = v;
		}

		public override void Serialize(QNetworkWriter writer) => writer.Write(value);

		public override void Deserialize(QNetworkReader reader) => value = reader.ReadString();
	}
}