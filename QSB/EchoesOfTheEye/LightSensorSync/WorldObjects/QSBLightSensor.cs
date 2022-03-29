using QSB.EchoesOfTheEye.LightSensorSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;
using System;

namespace QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;

internal class QSBLightSensor : WorldObject<SingleLightSensor>
{
	public bool IlluminatedByLocal;

	public Action OnDetectLocalLight;
	public Action OnDetectLocalDarkness;

	public override void SendInitialState(uint to) =>
		this.SendMessage(new SetEnabledMessage(AttachedObject.enabled));
}
