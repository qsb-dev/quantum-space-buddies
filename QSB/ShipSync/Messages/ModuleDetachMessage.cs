using QSB.Messaging;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages;

internal class ModuleDetachMessage : QSBWorldObjectMessage<QSBShipDetachableModule>
{
	public override void OnReceiveRemote() =>
		WorldObject.AttachedObject.Detach();
}
