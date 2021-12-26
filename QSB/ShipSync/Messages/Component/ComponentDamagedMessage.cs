using QSB.Messaging;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages.Component
{
	internal class ComponentDamagedMessage : QSBWorldObjectMessage<QSBShipComponent>
	{
		public override void OnReceiveRemote() => WorldObject.SetDamaged();
	}
}
