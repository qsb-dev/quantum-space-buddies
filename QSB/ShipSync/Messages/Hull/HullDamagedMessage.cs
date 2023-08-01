using QSB.Messaging;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages.Hull;

public class HullDamagedMessage : QSBWorldObjectMessage<QSBShipHull>
{
	public override void OnReceiveRemote() => WorldObject.SetDamaged();
}