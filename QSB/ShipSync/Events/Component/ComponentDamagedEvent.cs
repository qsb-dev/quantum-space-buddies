using QSB.Events;
using QSB.ShipSync.WorldObjects;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.ShipSync.Events.Component
{
	internal class ComponentDamagedEvent : QSBEvent<WorldObjectMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<ShipComponent>.AddListener(EventNames.QSBComponentDamaged, Handler);
		public override void CloseListener() => GlobalMessenger<ShipComponent>.RemoveListener(EventNames.QSBComponentDamaged, Handler);

		private void Handler(ShipComponent hull) => SendEvent(CreateMessage(hull));

		private WorldObjectMessage CreateMessage(ShipComponent hull)
		{
			var worldObject = hull.GetWorldObject<QSBShipComponent>();
			return new WorldObjectMessage
			{
				AboutId = LocalPlayerId,
				ObjectId = worldObject.ObjectId
			};
		}

		public override void OnReceiveRemote(bool server, WorldObjectMessage message)
		{
			var worldObject = QSBWorldSync.GetWorldFromId<QSBShipComponent>(message.ObjectId);
			worldObject.SetDamaged();
		}
	}
}
