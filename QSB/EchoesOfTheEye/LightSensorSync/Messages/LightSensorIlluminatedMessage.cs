using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.LightSensorSync.Messages;

internal class LightSensorIlluminatedMessage : QSBWorldObjectMessage<QSBLightSensor, bool>
{
	public LightSensorIlluminatedMessage(bool illuminated) : base(illuminated) { }
	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote()
	{
		if (Data)
		{
			WorldObject._illuminatedBy.SafeAdd(From);
		}
		else
		{
			WorldObject._illuminatedBy.QuickRemove(From);
		}
	}
}
