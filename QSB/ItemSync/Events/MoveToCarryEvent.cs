using QSB.Events;
using QSB.ItemSync.WorldObjects;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.ItemSync.Events
{
	internal class MoveToCarryEvent : QSBEvent<WorldObjectMessage>
	{
		public override EventType Type => EventType.MoveToCarry;

		public override void SetupListener()
			=> GlobalMessenger<int>.AddListener(EventNames.QSBMoveToCarry, Handler);

		public override void CloseListener()
			=> GlobalMessenger<int>.RemoveListener(EventNames.QSBMoveToCarry, Handler);

		private void Handler(int itemId)
			=> SendEvent(CreateMessage(itemId));

		private WorldObjectMessage CreateMessage(int itemid) => new WorldObjectMessage
		{
			AboutId = QSBPlayerManager.LocalPlayerId,
			ObjectId = itemid
		};

		public override void OnReceiveRemote(bool server, WorldObjectMessage message)
		{
			var itemObject = QSBWorldSync.GetWorldFromId<IQSBOWItem>(message.ObjectId);
			DebugLog.DebugWrite($"Pretend we pick up item {(itemObject as IWorldObject).Name} for player {message.FromId}");
		}
	}
}
