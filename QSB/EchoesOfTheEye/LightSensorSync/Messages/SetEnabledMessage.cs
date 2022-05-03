using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.LightSensorSync.Messages;

internal class SetEnabledMessage : QSBWorldObjectMessage<QSBLightSensor, bool>
{
	public SetEnabledMessage(bool data) : base(data) { }
	public override void OnReceiveRemote() => WorldObject.SetEnabled(Data);
}
