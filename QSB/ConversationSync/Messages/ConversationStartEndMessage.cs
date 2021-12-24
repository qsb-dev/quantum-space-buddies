using QSB.Messaging;
using QuantumUNET.Transport;

namespace QSB.ConversationSync.Messages
{
	public class ConversationStartEndMessage : PlayerMessage
	{
		public int TreeId { get; set; }
		public uint PlayerId { get; set; }
		public bool State { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			TreeId = reader.ReadInt32();
			PlayerId = reader.ReadUInt32();
			State = reader.ReadBoolean();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(TreeId);
			writer.Write(PlayerId);
			writer.Write(State);
		}
	}
}