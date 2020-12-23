using QuantumUNET.Transport;

namespace QSB.Messaging
{
	public class FloatMessage : PlayerMessage
	{
		public float Value;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Value = reader.ReadSingle();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Value);
		}
	}
}