using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.ConversationSync.Messages
{
	public class ConversationMessage : EnumWorldObjectMessage<ConversationType>
	{
		public string Message { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Message = reader.ReadString();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Message);
		}
	}
}