using OWML.Common;
using QSB.Messaging;
using QSB.Player.Messages;
using QSB.Utility;

namespace QSB.WorldSync;

internal class HashMessage : QSBMessage<int>
{
	public HashMessage(int hash) : base(hash) => To = 0;

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
