using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Player;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

public class ContactTriggerMessage : QSBWorldObjectMessage<QSBGhostSensors, bool>
{
	public ContactTriggerMessage(bool inContact) : base(inContact) { }

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);

		if (WorldObject._data.players[player] != null && WorldObject._data.players[player].sensor != null)
		{
			WorldObject._data.players[player].sensor.inContactWithPlayer = Data;
		}
	}
}
