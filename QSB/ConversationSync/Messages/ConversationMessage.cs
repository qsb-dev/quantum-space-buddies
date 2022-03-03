using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System.Text.RegularExpressions;
using UnityEngine;

namespace QSB.ConversationSync.Messages;

public class ConversationMessage : QSBMessage<(ConversationType Type, int Id, string Message)>
{
	public ConversationMessage(ConversationType type, int id, string message = "")
	{
		Data.Type = type;
		Data.Id = id;
		Data.Message = message;
	}

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		switch (Data.Type)
		{
			case ConversationType.Character:
				var translated = TextTranslation.Translate(Data.Message).Trim();
				translated = Regex.Replace(translated, @"<[Pp]ause=?\d*\.?\d*\s?\/?>", "");
				ConversationManager.Instance.DisplayCharacterConversationBox(Data.Id, translated);
				break;

			case ConversationType.Player:
				ConversationManager.Instance.DisplayPlayerConversationBox((uint)Data.Id, Data.Message);
				break;

			case ConversationType.CloseCharacter:
				if (Data.Id == -1)
				{
					break;
				}

				var tree = QSBWorldSync.OldDialogueTrees[Data.Id];
				Object.Destroy(ConversationManager.Instance.BoxMappings[tree]);
				break;

			case ConversationType.ClosePlayer:
				Object.Destroy(QSBPlayerManager.GetPlayer((uint)Data.Id).CurrentDialogueBox);
				break;
		}
	}
}