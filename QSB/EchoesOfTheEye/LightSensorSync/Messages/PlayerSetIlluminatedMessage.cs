using QSB.Messaging;
using QSB.Player;

namespace QSB.EchoesOfTheEye.LightSensorSync.Messages;

internal class PlayerSetIlluminatedMessage : QSBMessage<(uint playerId, bool illuminated)>
{
	public PlayerSetIlluminatedMessage(uint playerId, bool illuminated) : base((playerId, illuminated)) { }
	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote() =>
		QSBPlayerManager.GetPlayer(Data.playerId).QSBPlayerLightSensor.SetIlluminated(From, Data.illuminated);
}
