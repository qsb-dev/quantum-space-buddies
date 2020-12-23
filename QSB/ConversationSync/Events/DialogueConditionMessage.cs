using QSB.Messaging;
using QuantumUNET.Transport;

namespace QSB.ConversationSync.Events
{
	public class DialogueConditionMessage : PlayerMessage
	{
		public string ConditionName { get; set; }
		public bool ConditionState { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			ConditionName = reader.ReadString();
			ConditionState = reader.ReadBoolean();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ConditionName);
			writer.Write(ConditionState);
		}
	}
}
