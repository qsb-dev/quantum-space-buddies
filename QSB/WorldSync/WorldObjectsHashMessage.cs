using OWML.Common;
using QSB.Messaging;
using QSB.Player.Messages;
using QSB.Utility;

namespace QSB.WorldSync;

/// <summary>
/// sends QSBWorldSync.WorldObjectsHash to the server for sanity checking
/// </summary>
internal class WorldObjectsHashMessage : QSBMessage<string>
{
	public WorldObjectsHashMessage() : base(QSBWorldSync.WorldObjectsHash) => To = 0;

	public override void OnReceiveRemote()
	{
		var serverHash = QSBWorldSync.WorldObjectsHash;

		if (serverHash != Data)
		{
			// oh fuck oh no oh god
			DebugLog.ToConsole($"Kicking {From} because their WorldObjects hash is wrong. (server:{serverHash}, client:{Data})", MessageType.Error);
			new PlayerKickMessage(From, $"WorldObject hash error. (Server:{serverHash}, Client:{Data})").Send();
		}
		else
		{
			DebugLog.DebugWrite($"WorldObject hash from {From} verified!", MessageType.Success);
		}
	}
}
