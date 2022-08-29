using QSB.Messaging;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages;

internal class ShipLightMessage : QSBWorldObjectMessage<QSBShipLight, bool>
{
	public ShipLightMessage(bool on) : base(on) { }

	public override void OnReceiveRemote() =>
		WorldObject.AttachedObject.SetOn(Data);
}
