using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.LightSensorSync.Messages;

internal class SetIlluminatedMessage : QSBWorldObjectMessage<QSBLightSensor, bool>
{
	public SetIlluminatedMessage(bool illuminated) : base(illuminated) { }
	public override void OnReceiveLocal() => OnReceiveRemote();
	public override void OnReceiveRemote() => WorldObject.SetIlluminated(From, Data);
}
