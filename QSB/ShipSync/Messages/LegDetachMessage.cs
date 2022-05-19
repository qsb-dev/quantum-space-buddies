using QSB.Messaging;
using QSB.Patches;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages;

internal class LegDetachMessage : QSBWorldObjectMessage<QSBShipDetachableLeg>
{
	public override void OnReceiveRemote() =>
		QSBPatch.RemoteCall(WorldObject.AttachedObject.Detach);
}
