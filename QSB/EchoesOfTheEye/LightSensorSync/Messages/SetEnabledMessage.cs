using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.LightSensorSync.Messages;

internal class SetEnabledMessage : QSBWorldObjectMessage<QSBLightSensor, bool>
{
	public SetEnabledMessage(bool data) : base(data) { }
	public override void OnReceiveRemote() =>
		QSBPatch.RemoteCall(WorldObject.AttachedObject.OnSectorOccupantsUpdated, Data);
}
