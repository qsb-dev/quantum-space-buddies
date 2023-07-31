using QSB.Messaging;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages;

public class ModuleDetachMessage : QSBWorldObjectMessage<QSBShipDetachableModule>
{
	public override void OnReceiveRemote() =>
		WorldObject.AttachedObject.Detach();
}
