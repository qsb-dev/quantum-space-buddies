using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;

namespace QSB.EchoesOfTheEye.LightSensorSync.Messages;

internal class PlayerIlluminatingLanternsMessage : QSBMessage<(uint playerId, int[] lanterns)>
{
	public PlayerIlluminatingLanternsMessage(uint playerId, IEnumerable<DreamLanternController> lanterns) :
		base((
			playerId,
			lanterns.Select(x => x.GetWorldObject<QSBDreamLanternController>().ObjectId).ToArray()
		))
	{ }

	public override void OnReceiveRemote()
	{
		var lightSensor = (SingleLightSensor)QSBPlayerManager.GetPlayer(Data.playerId).LightSensor;

		lightSensor._illuminatingDreamLanternList.Clear();
		lightSensor._illuminatingDreamLanternList.AddRange(
			Data.lanterns.Select(x => x.GetWorldObject<QSBDreamLanternController>().AttachedObject));
	}
}
