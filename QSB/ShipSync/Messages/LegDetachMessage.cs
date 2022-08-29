using QSB.Messaging;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages;

internal class LegDetachMessage : QSBWorldObjectMessage<QSBShipDetachableLeg>
{
	public override void OnReceiveRemote() =>
		WorldObject.AttachedObject.Detach();
}
