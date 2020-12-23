using QuantumUNET.Transport;

namespace QSB.Messaging
{
	public class ToggleMessage : PlayerMessage
	{
		public bool ToggleValue { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			ToggleValue = reader.ReadBoolean();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ToggleValue);
		}
	}
}