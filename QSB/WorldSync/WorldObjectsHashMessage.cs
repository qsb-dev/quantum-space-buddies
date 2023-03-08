using OWML.Common;
using QSB.Messaging;
using QSB.Player.Messages;
using QSB.Utility;

namespace QSB.WorldSync;

/// <summary>
/// sends QSBWorldSync.WorldObjectsHash to the server for sanity checking
/// </summary>
internal class WorldObjectsHashMessage : QSBMessage<(string managerName, string hash, int count)>
{
	public WorldObjectsHashMessage(string managerName, string hash, int count) : base((managerName, hash, count)) => To = 0;

	public override void OnReceiveRemote()
	{
		Delay.RunWhen(() => QSBWorldSync.AllObjectsAdded, () =>
		{
			var (hash, count) = QSBWorldSync.ManagerHashes[Data.managerName];

			if (hash != Data.hash)
			{
				// oh fuck oh no oh god
				DebugLog.ToConsole($"Kicking {From} because their WorldObjects hash for {Data.managerName} is wrong. (Server:{hash} count:{count}, Client:{Data.hash} count:{Data.count})", MessageType.Error);
				new PlayerKickMessage(From, $"WorldObject hash error for {Data.managerName}. (Server:{hash} count:{count}, Client:{Data.hash}, count:{Data.count})").Send();
			}
		});
	}
}
