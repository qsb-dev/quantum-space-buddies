using QSB.Anglerfish.WorldObjects;
using QSB.Events;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.Anglerfish.Events
{
	public class AnglerUnsuspendEvent : QSBEvent<WorldObjectMessage>
	{
		public override EventType Type => EventType.AnglerUnsuspend;
		public override void SetupListener() => GlobalMessenger<int>.AddListener(EventNames.QSBAnglerUnsuspend, Handler);
		public override void CloseListener() => GlobalMessenger<int>.RemoveListener(EventNames.QSBAnglerUnsuspend, Handler);
		private void Handler(int id) =>
			SendEvent(new WorldObjectMessage
			{
				ObjectId = id,
				OnlySendToHost = true
			});

		public override void OnReceiveLocal(bool isHost, WorldObjectMessage message) =>
			QSBWorldSync.GetWorldFromId<QSBAngler>(message.ObjectId).TransferAuthority(message.FromId);
		public override void OnReceiveRemote(bool isHost, WorldObjectMessage message) =>
			QSBWorldSync.GetWorldFromId<QSBAngler>(message.ObjectId).TransferAuthority(message.FromId);
	}
}
