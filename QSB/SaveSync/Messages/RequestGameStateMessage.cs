using QSB.ConversationSync.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.ServerSettings;
using QSB.Utility;

namespace QSB.SaveSync.Messages;

/// <summary>
/// always sent to host
/// </summary>
public class RequestGameStateMessage : QSBMessage
{
	public RequestGameStateMessage() => To = 0;

	public override void OnReceiveRemote() => Delay.RunFramesLater(100, () =>
	{
		if (!QSBPlayerManager.PlayerExists(From))
		{
			// player was kicked
			return;
		}

		new GameStateMessage(From).Send();
		new ServerSettingsMessage().Send();

		var gameSave = PlayerData._currentGameSave;

		var factSaves = gameSave.shipLogFactSaves;
		foreach (var item in factSaves)
		{
			new ShipLogFactSaveMessage(item.Value).Send();
		}

		var dictConditions = gameSave.dictConditions;
		foreach (var item in dictConditions)
		{
			new PersistentConditionMessage(item.Key, item.Value).Send();
		}
	});
}