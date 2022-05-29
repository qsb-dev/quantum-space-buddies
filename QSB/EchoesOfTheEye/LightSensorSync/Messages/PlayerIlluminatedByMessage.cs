using QSB.Messaging;
using QSB.Player;
using System.Linq;

namespace QSB.EchoesOfTheEye.LightSensorSync.Messages;

/// <summary>
/// always sent by host
/// </summary>
internal class PlayerIlluminatedByMessage : QSBMessage<(uint playerId, uint[] illuminatedBy)>
{
	public PlayerIlluminatedByMessage(uint playerId, uint[] illuminatedBy) : base((playerId, illuminatedBy)) { }

	public override void OnReceiveRemote()
	{
		var qsbPlayerLightSensor = QSBPlayerManager.GetPlayer(Data.playerId).QSBPlayerLightSensor;

		foreach (var added in Data.illuminatedBy.Except(qsbPlayerLightSensor._illuminatedBy))
		{
			qsbPlayerLightSensor.SetIlluminated(added, true);
		}

		foreach (var removed in qsbPlayerLightSensor._illuminatedBy.Except(Data.illuminatedBy))
		{
			qsbPlayerLightSensor.SetIlluminated(removed, false);
		}
	}
}
