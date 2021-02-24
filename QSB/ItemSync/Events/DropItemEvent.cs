using QSB.Events;
using QSB.ItemSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.Events
{
	internal class DropItemEvent : QSBEvent<DropItemMessage>
	{
		public override QSB.Events.EventType Type => QSB.Events.EventType.DropItem;

		public override void SetupListener()
			=> GlobalMessenger<int, Vector3, Vector3, Transform, Sector, DetachableFragment>.AddListener(EventNames.QSBDropItem, Handler);

		public override void CloseListener()
			=> GlobalMessenger<int, Vector3, Vector3, Transform, Sector, DetachableFragment>.RemoveListener(EventNames.QSBDropItem, Handler);

		private void Handler(int objectId, Vector3 position, Vector3 normal, Transform parent, Sector sector, DetachableFragment fragment)
			=> SendEvent(CreateMessage(objectId, position, normal, parent, sector, fragment));

		private DropItemMessage CreateMessage(int objectId, Vector3 position, Vector3 normal, Transform parent, Sector sector, DetachableFragment fragment) => new DropItemMessage
		{
			ObjectId = objectId,
			Position = position,
			Normal = normal,
			Parent = parent,
			Sector = sector,
			DetachableFragment = fragment
		};

		public override void OnReceiveRemote(bool server, DropItemMessage message)
		{
			var worldObject = QSBWorldSync.GetWorldFromId<IQSBOWItem>(message.ObjectId);
		}
	}
}
