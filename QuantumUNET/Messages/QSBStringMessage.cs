namespace QuantumUNET.Messages
{
	public class QSBStringMessage : QSBMessageBase
	{
		public QSBStringMessage()
		{
		}

		public QSBStringMessage(string v)
		{
			value = v;
		}

		public override void Deserialize(QSBNetworkReader reader) => value = reader.ReadString();

		public override void Serialize(QSBNetworkWriter writer) => writer.Write(value);

		public string value;
	}
}