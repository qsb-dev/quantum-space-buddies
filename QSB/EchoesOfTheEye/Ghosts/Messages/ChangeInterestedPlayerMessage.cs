using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Player;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

internal class ChangeInterestedPlayerMessage : QSBWorldObjectMessage<QSBGhostSensors, uint>
{
	public ChangeInterestedPlayerMessage(uint playerId) : base(playerId) { }

	public override void OnReceiveRemote()
		=> WorldObject._data.interestedPlayer = WorldObject._data.players[QSBPlayerManager.GetPlayer(Data)];
}
