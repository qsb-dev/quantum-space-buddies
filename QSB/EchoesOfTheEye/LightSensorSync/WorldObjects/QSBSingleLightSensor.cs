using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;

// will be implemented when eote
internal class QSBSingleLightSensor : WorldObject<SingleLightSensor>
{
	public override void SendInitialState(uint to) { }

	public bool IlluminatedByLocalPlayer;
}