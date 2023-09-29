using QSB.Messaging;
using QSB.ShipSync.Messages;
using QSB.WorldSync;

namespace QSB.ShipSync.WorldObjects;

public class QSBShipLight : WorldObject<ShipLight>
{
	public override void SendInitialState(uint to) =>
		this.SendMessage(new ShipLightMessage(AttachedObject._on) { To = to });
}
