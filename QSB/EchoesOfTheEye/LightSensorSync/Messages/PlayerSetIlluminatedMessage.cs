using QSB.Messaging;
using QSB.Player;

namespace QSB.EchoesOfTheEye.LightSensorSync.Messages;

public class PlayerSetIlluminatedMessage : QSBMessage<(uint playerId, bool illuminated)>
{
	public PlayerSetIlluminatedMessage(uint playerId, bool illuminated) : base((playerId, illuminated)) { }

	public override void OnReceiveRemote()
	{
		var lightSensor = (SingleLightSensor)QSBPlayerManager.GetPlayer(Data.playerId).LightSensor;

		if (lightSensor._illuminated == Data.illuminated)
		{
			return;
		}

		lightSensor._illuminated = Data.illuminated;
		if (Data.illuminated)
		{
			lightSensor.OnDetectLight.Invoke();
		}
		else
		{
			lightSensor.OnDetectDarkness.Invoke();
		}
	}
}
