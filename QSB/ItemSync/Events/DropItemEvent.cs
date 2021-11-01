using QSB.Events;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.Events
{
	internal class DropItemEvent : QSBEvent<DropItemMessage>
	{
		public override QSB.Events.EventType Type => QSB.Events.EventType.DropItem;

		public override void SetupListener()
			=> GlobalMessenger<int, Vector3, Vector3, Sector>.AddListener(EventNames.QSBDropItem, Handler);

		public override void CloseListener()
			=> GlobalMessenger<int, Vector3, Vector3, Sector>.RemoveListener(EventNames.QSBDropItem, Handler);

		private void Handler(int objectId, Vector3 position, Vector3 normal, Sector sector)
			=> SendEvent(CreateMessage(objectId, position, normal, sector));

		private DropItemMessage CreateMessage(int objectId, Vector3 position, Vector3 normal, Sector sector) => new DropItemMessage
		{
			ObjectId = objectId,
			Position = position,
			Normal = normal,
			Sector = sector
		};

		public override void OnReceiveRemote(bool server, DropItemMessage message)
		{
			var worldObject = QSBWorldSync.GetWorldFromId<IQSBOWItem>(message.ObjectId);
			worldObject.DropItem(message.Position, message.Normal, message.Sector);

			var player = QSBPlayerManager.GetPlayer(message.FromId);
			player.HeldItem = worldObject;
		}
	}
}
