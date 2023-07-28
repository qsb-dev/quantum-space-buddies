using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

public class FacePlayerMessage : QSBWorldObjectMessage<QSBGhostController, (uint playerId, TurnSpeed turnSpeed)>
{
	public FacePlayerMessage(uint playerId, TurnSpeed turnSpeed) : base((playerId, turnSpeed)) { }

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			DebugLog.ToConsole("Error - Received FacePlayerMessage on host. Something has gone horribly wrong!", OWML.Common.MessageType.Error);
			return;
		}

		WorldObject.FacePlayer(QSBPlayerManager.GetPlayer(Data.playerId), Data.turnSpeed, true);
	}
}
