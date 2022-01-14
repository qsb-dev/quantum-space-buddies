using Mirror;
using QSB.ConversationSync.WorldObjects;
using QSB.Messaging;

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

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ActivatedDialogueIndex);
			writer.Write(ListDialoguesIndex);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			ActivatedDialogueIndex = reader.Read<int>();
			ListDialoguesIndex = reader.Read<int>();
		}

		public override void OnReceiveRemote()
			=> WorldObject.RemoteEnterDialogue(ActivatedDialogueIndex, ListDialoguesIndex);
	}
}