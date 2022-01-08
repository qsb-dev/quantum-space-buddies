using QSB.Messaging;
using QSB.Utility;

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
			DebugLog.DebugWrite($"GET REQUEST FOR GAME STATE");

			new GameStateMessage(From).Send();

			var gameSave = StandaloneProfileManager.SharedInstance.currentProfileGameSave;

			var factSaves = gameSave.shipLogFactSaves;
			foreach (var item in factSaves)
			{
				new ShipLogFactSaveMessage(item.Value).Send();
			}
		}
	}
}
