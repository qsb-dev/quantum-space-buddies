using Mirror;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System.Text.RegularExpressions;
using UnityEngine;

namespace QSB.ConversationSync.Messages
{
	public class ConversationMessage : QSBMessage<ConversationType>
	{
		private int Id;
		private string Message;

		public ConversationMessage(ConversationType type, int id, string message = "")
		{
			Value = type;
			Id = id;
			Message = message;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Id);
			writer.Write(Message);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			Id = reader.Read<int>();
			Message = reader.ReadString();
		}

		public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			switch (Value)
			{
				case ConversationType.Character:
					var translated = TextTranslation.Translate(Message).Trim();
					translated = Regex.Replace(translated, @"<[Pp]ause=?\d*\.?\d*\s?\/?>", "");
					ConversationManager.Instance.DisplayCharacterConversationBox(Id, translated);
					break;

				case ConversationType.Player:
					ConversationManager.Instance.DisplayPlayerConversationBox((uint)Id, Message);
					break;

				case ConversationType.CloseCharacter:
					if (Id == -1)
					{
						break;
					}

					var tree = QSBWorldSync.OldDialogueTrees[Id];
					Object.Destroy(ConversationManager.Instance.BoxMappings[tree]);
					break;

				case ConversationType.ClosePlayer:
					Object.Destroy(QSBPlayerManager.GetPlayer((uint)Id).CurrentDialogueBox);
					break;
			}
		}
	}
}