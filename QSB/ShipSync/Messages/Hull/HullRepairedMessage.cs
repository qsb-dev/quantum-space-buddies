using QSB.Messaging;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages.Hull;

public class HullRepairedMessage : QSBWorldObjectMessage<QSBShipHull>
{
	public override void OnReceiveRemote() => WorldObject.SetRepaired();
}