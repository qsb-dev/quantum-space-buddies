using QSB.WorldSync;
using System;

namespace QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;

internal class QSBLightSensor : WorldObject<SingleLightSensor>
{
	public bool LocallyIlluminated;

	public Action OnDetectLocalLight;
	public Action OnDetectLocalDarkness;

	public override void SendInitialState(uint to) { }
}
