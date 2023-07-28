using QSB.Messaging;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages.Component;

public class ComponentDamagedMessage : QSBWorldObjectMessage<QSBShipComponent>
{
	public override void OnReceiveRemote() => WorldObject.SetDamaged();
}