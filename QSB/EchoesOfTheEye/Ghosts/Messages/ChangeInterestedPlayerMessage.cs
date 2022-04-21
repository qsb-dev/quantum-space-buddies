using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

internal class ChangeInterestedPlayerMessage : QSBWorldObjectMessage<QSBGhostSensors, uint>
{
	public ChangeInterestedPlayerMessage(uint playerId) : base(playerId) { }

	public override void OnReceiveRemote()
	{
		DebugLog.DebugWrite($"{WorldObject.AttachedObject.name} Set interested player {Data}");
		WorldObject._data.interestedPlayer = WorldObject._data.players[QSBPlayerManager.GetPlayer(Data)];
	}
}
