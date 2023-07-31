using QSB.Messaging;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages;

public class LegDetachMessage : QSBWorldObjectMessage<QSBShipDetachableLeg>
{
	public override void OnReceiveRemote() =>
		WorldObject.AttachedObject.Detach();
}
