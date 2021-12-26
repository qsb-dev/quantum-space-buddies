using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using QuantumUNET.Transport;
using System.Text.RegularExpressions;

namespace QSB.ConversationSync.Messages
{
	public class ConversationMessage : QSBEnumMessage<ConversationType>
	{
		/// <summary>
		/// character (tree) id or player id
		/// </summary>
		private int Id;
		/// <summary>
		/// used only for character and player type
		/// </summary>
		private string Message;

		public ConversationMessage(ConversationType type, int id, string message = default)
		{
			Value = type;
			Id = id;
			if (Value is ConversationType.Character or ConversationType.Player)
			{
				Message = message;
			}
		}

		public ConversationMessage() { }

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Id);
			if (Value is ConversationType.Character or ConversationType.Player)
			{
				writer.Write(Message);
			}
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Id = reader.ReadInt32();
			if (Value is ConversationType.Character or ConversationType.Player)
			{
				Message = reader.ReadString();
			}
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

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
					UnityEngine.Object.Destroy(ConversationManager.Instance.BoxMappings[tree]);
					break;

				case ConversationType.ClosePlayer:
					UnityEngine.Object.Destroy(QSBPlayerManager.GetPlayer((uint)Id).CurrentDialogueBox);
					break;
			}
		}
	}
}