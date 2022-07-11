using QSB.Messaging;
using QSB.Patches;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages;

internal class ModuleDetachMessage : QSBWorldObjectMessage<QSBShipDetachableModule>
{
	public override void OnReceiveRemote() =>
		QSBPatch.RemoteCall(WorldObject.AttachedObject.Detach);
}
