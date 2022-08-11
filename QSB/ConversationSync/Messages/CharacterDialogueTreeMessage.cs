using QSB.ConversationSync.WorldObjects;
using QSB.Messaging;

namespace QSB.ConversationSync.Messages
{
	internal class CharacterDialogueTreeMessage : QSBWorldObjectMessage<QSBCharacterDialogueTree, string>
	{
		public CharacterDialogueTreeMessage(string currentNodeName) : base(currentNodeName) { }

		public override void OnReceiveRemote() => WorldObject.SetInConversation(Data);		
	}
}
