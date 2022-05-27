using QSB.WorldSync;
using System;

namespace QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;

internal class QSBLightSensor : WorldObject<SingleLightSensor>
{
	/// <summary>
	/// illuminated specifically by the player
	/// </summary>
	public bool LocallyIlluminated;

	public Action OnDetectLocalLight;
	public Action OnDetectLocalDarkness;

	/// <summary>
	/// illuminated on the client (replacement for base game code)
	/// </summary>
	internal bool _illuminated;

	public override void SendInitialState(uint to) { }
}
