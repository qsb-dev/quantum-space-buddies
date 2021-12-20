using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.ConversationSync.Events
{
	public class EnterRemoteDialogueMessage : WorldObjectMessage
	{
		public int ActivatedDialogueIndex { get; set; }
		public int ListDialoguesIndex { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			ActivatedDialogueIndex = reader.ReadInt32();
			ListDialoguesIndex = reader.ReadInt32();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ActivatedDialogueIndex);
			writer.Write(ListDialoguesIndex);
		}
	}
}