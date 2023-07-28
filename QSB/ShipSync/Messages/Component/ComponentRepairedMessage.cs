using QSB.Messaging;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages.Component;

public class ComponentRepairedMessage : QSBWorldObjectMessage<QSBShipComponent>
{
	public override void OnReceiveRemote() => WorldObject.SetRepaired();
}