namespace QSB.QuantumUNET
{
	public class QSBStringMessage : QSBMessageBase
	{
		public QSBStringMessage()
		{
		}

		public QSBStringMessage(string v)
		{
			this.value = v;
		}

		public override void Deserialize(QSBNetworkReader reader)
		{
			this.value = reader.ReadString();
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.Write(this.value);
		}

		public string value;
	}
}