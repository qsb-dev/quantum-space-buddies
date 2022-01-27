using Mirror;
using QSB.ConversationSync.WorldObjects;
using QSB.Messaging;

namespace QSB.ConversationSync.Messages
{
	internal class EnterRemoteDialogueMessage : QSBWorldObjectMessage<QSBRemoteDialogueTrigger>
	{
		private int DialogueIndex;

		public EnterRemoteDialogueMessage(int dialogueIndex)
			=> DialogueIndex = dialogueIndex;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(DialogueIndex);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			DialogueIndex = reader.Read<int>();
		}

		public override void OnReceiveRemote()
			=> WorldObject.RemoteEnterDialogue(DialogueIndex);
	}
}