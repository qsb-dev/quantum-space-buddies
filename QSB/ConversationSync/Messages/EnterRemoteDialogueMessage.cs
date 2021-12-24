using QSB.ConversationSync.WorldObjects;
using QSB.Messaging;
using QuantumUNET.Transport;

namespace QSB.ConversationSync.Messages
{
	internal class EnterRemoteDialogueMessage : QSBWorldObjectMessage<QSBRemoteDialogueTrigger>
	{
		private int ActivatedDialogueIndex;
		private int ListDialoguesIndex;

		public EnterRemoteDialogueMessage(int activatedIndex, int listIndex)
		{
			ActivatedDialogueIndex = activatedIndex;
			ListDialoguesIndex = listIndex;
		}

		public EnterRemoteDialogueMessage() { }

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ActivatedDialogueIndex);
			writer.Write(ListDialoguesIndex);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			ActivatedDialogueIndex = reader.ReadInt32();
			ListDialoguesIndex = reader.ReadInt32();
		}

		public override void OnReceiveRemote()
			=> WorldObject.RemoteEnterDialogue(ActivatedDialogueIndex, ListDialoguesIndex);
	}
}