using QSB.ConversationSync.Messages;
using QSB.ConversationSync.Patches;
using QSB.Messaging;
using System.Linq;

namespace QSB.SaveSync.Messages
{
	/// <summary>
	/// always sent to host
	/// </summary>
	internal class RequestGameStateMessage : QSBMessage
	{
		public RequestGameStateMessage() => To = 0;

		public override void OnReceiveRemote()
		{
			new GameStateMessage(From).Send();

			var gameSave = StandaloneProfileManager.SharedInstance.currentProfileGameSave;

			var factSaves = gameSave.shipLogFactSaves;
			foreach (var item in factSaves)
			{
				new ShipLogFactSaveMessage(item.Value).Send();
			}

			var dictConditions = gameSave.dictConditions;
			var dictConditionsToSend = dictConditions.Where(x => ConversationPatches.PersistentConditionsToSync.Contains(x.Key));
			foreach (var item in dictConditionsToSend)
			{
				new PersistentConditionMessage(item.Key, item.Value).Send();
			}
		}
	}
}
