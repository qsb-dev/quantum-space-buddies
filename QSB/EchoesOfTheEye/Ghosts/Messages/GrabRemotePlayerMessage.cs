using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

public class GrabRemotePlayerMessage : QSBWorldObjectMessage<QSBGhostGrabController, (float speed, uint playerId)>
{
	public GrabRemotePlayerMessage(float speed, uint playerId) : base((speed, playerId)) { }

	public override void OnReceiveRemote()
	{
		var allGhosts = QSBWorldSync.GetWorldObjects<QSBGhostBrain>();
		var owningGhost = allGhosts.First(x => x.AttachedObject._controller == WorldObject.AttachedObject._effects._controller);
		WorldObject.GrabPlayer(Data.speed, owningGhost._data.players[QSBPlayerManager.GetPlayer(Data.playerId)], true);
	}
}
