using System.Text.RegularExpressions;
using QSB.Events;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.ConversationSync.Messages
{
	public class ConversationEvent : QSBEvent<ConversationMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<uint, string, ConversationType>.AddListener(EventNames.QSBConversation, Handler);
		public override void CloseListener() => GlobalMessenger<uint, string, ConversationType>.RemoveListener(EventNames.QSBConversation, Handler);

		private void Handler(uint id, string message, ConversationType type) => SendEvent(CreateMessage(id, message, type));

		private ConversationMessage CreateMessage(uint id, string message, ConversationType type) => new()
		{
			AboutId = LocalPlayerId,
			ObjectId = (int)id,
			EnumValue = type,
			Message = message
		};

		public override void OnReceiveRemote(bool server, ConversationMessage message)
		{
			switch (message.EnumValue)
			{
				case ConversationType.Character:
					var translated = TextTranslation.Translate(message.Message).Trim();
					translated = Regex.Replace(translated, @"<[Pp]ause=?\d*\.?\d*\s?\/?>", "");
					ConversationManager.Instance.DisplayCharacterConversationBox(message.ObjectId, translated);
					break;

				case ConversationType.Player:
					ConversationManager.Instance.DisplayPlayerConversationBox((uint)message.ObjectId, message.Message);
					break;

				case ConversationType.CloseCharacter:
					if (message.ObjectId == -1)
					{
						break;
					}

					var tree = QSBWorldSync.OldDialogueTrees[message.ObjectId];
					UnityEngine.Object.Destroy(ConversationManager.Instance.BoxMappings[tree]);
					break;

				case ConversationType.ClosePlayer:
					UnityEngine.Object.Destroy(QSBPlayerManager.GetPlayer((uint)message.ObjectId).CurrentDialogueBox);
					break;
			}
		}
	}
}