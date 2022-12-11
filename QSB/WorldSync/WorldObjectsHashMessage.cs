using OWML.Common;
using QSB.Messaging;
using QSB.Player.Messages;
using QSB.Utility;

namespace QSB.WorldSync;

/// <summary>
/// sends QSBWorldSync.WorldObjectsHash to the server for sanity checking
/// </summary>
internal class WorldObjectsHashMessage : QSBMessage<(string managerName, string hash)>
{
	public WorldObjectsHashMessage(string managerName, string hash) : base((managerName, hash)) => To = 0;

	public override void OnReceiveRemote()
	{
		Delay.RunWhen(() => QSBWorldSync.AllObjectsReady, () =>
		{
			var serverHash = QSBWorldSync.ManagerHashes[Data.managerName];

			if (serverHash != Data.hash)
			{
				// oh fuck oh no oh god
				DebugLog.ToConsole($"Kicking {From} because their WorldObjects hash for {Data.managerName} is wrong. (server:{serverHash}, client:{Data.hash})", MessageType.Error);
				new PlayerKickMessage(From, $"WorldObject hash error for {Data.managerName}. (Server:{serverHash}, Client:{Data.hash})").Send();
			}
		});
	}
}
