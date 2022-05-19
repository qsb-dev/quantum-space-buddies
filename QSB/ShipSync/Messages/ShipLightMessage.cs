using QSB.Messaging;
using QSB.Patches;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages;

internal class ShipLightMessage : QSBWorldObjectMessage<QSBShipLight, bool>
{
	public ShipLightMessage(bool on) : base(on) { }

	public override void OnReceiveRemote() =>
		QSBPatch.RemoteCall(() => WorldObject.AttachedObject.SetOn(Data));
}
