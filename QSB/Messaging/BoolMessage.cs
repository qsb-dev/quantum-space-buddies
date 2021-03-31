using QuantumUNET.Transport;

namespace QSB.Messaging
{
	public class BoolMessage : PlayerMessage
	{
		public bool Value;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Value = reader.ReadBoolean();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Value);
		}
	}
}