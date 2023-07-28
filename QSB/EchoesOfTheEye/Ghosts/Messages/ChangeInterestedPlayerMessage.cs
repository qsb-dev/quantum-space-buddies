using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

public class ChangeInterestedPlayerMessage : QSBWorldObjectMessage<QSBGhostSensors, uint>
{
	public ChangeInterestedPlayerMessage(uint playerId) : base(playerId) { }

	public override void OnReceiveRemote()
		=> WorldObject._data.interestedPlayer = WorldObject._data.players[QSBPlayerManager.GetPlayer(Data)];
}
