using Mirror;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System.Text.RegularExpressions;
using UnityEngine;

namespace QSB.ConversationSync.Messages
{
	public class ConversationMessage : QSBMessage<ConversationType, int, string>
	{
		public ConversationMessage(ConversationType type, int id, string message = "")
		{
			Value1 = type;
			Value2 = id;
			Value3 = message;
		}

		public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			switch (Value1)
			{
				case ConversationType.Character:
					var translated = TextTranslation.Translate(Value3).Trim();
					translated = Regex.Replace(translated, @"<[Pp]ause=?\d*\.?\d*\s?\/?>", "");
					ConversationManager.Instance.DisplayCharacterConversationBox(Value2, translated);
					break;

				case ConversationType.Player:
					ConversationManager.Instance.DisplayPlayerConversationBox((uint)Value2, Value3);
					break;

				case ConversationType.CloseCharacter:
					if (Id == -1)
					{
						break;
					}

					var tree = QSBWorldSync.OldDialogueTrees[Value2];
					Object.Destroy(ConversationManager.Instance.BoxMappings[tree]);
					break;

				case ConversationType.ClosePlayer:
					Object.Destroy(QSBPlayerManager.GetPlayer((uint)Value2).CurrentDialogueBox);
					break;
			}
		}
	}
}