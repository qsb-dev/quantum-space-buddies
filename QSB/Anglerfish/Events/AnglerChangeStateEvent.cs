using QSB.Anglerfish.WorldObjects;
using QSB.Events;
using QSB.WorldSync;
using static AnglerfishController;

namespace QSB.Anglerfish.Events
{
	public class AnglerChangeStateEvent : QSBEvent<AnglerChangeStateMessage>
	{
		public override EventType Type => EventType.AnglerChangeState;
		public override void SetupListener() => GlobalMessenger<QSBAngler>.AddListener(EventNames.QSBAnglerChangeState, Handler);
		public override void CloseListener() => GlobalMessenger<QSBAngler>.RemoveListener(EventNames.QSBAnglerChangeState, Handler);
		private void Handler(QSBAngler qsbAngler) =>
			SendEvent(new AnglerChangeStateMessage
			{
				ObjectId = qsbAngler.ObjectId,
				EnumValue = qsbAngler.AttachedObject._currentState,
				targetId = qsbAngler.TargetToId(),
				localDisturbancePos = qsbAngler.AttachedObject._localDisturbancePos
			});

		public override void OnReceiveLocal(bool isHost, AnglerChangeStateMessage message) => OnReceive(isHost, message);
		public override void OnReceiveRemote(bool isHost, AnglerChangeStateMessage message) => OnReceive(isHost, message);
		private static void OnReceive(bool isHost, AnglerChangeStateMessage message)
		{
			var qsbAngler = QSBWorldSync.GetWorldFromId<QSBAngler>(message.ObjectId);

			if (isHost &&
				(message.EnumValue == AnglerState.Chasing || message.EnumValue == AnglerState.Consuming || message.EnumValue == AnglerState.Investigating))
			{
				qsbAngler.TransferAuthority(message.FromId);
			}

			qsbAngler.AttachedObject.enabled = qsbAngler.transformSync.HasAuthority;
			qsbAngler.AttachedObject._currentState = message.EnumValue;
			qsbAngler.target = QSBAngler.IdToTarget(message.targetId);
			qsbAngler.AttachedObject._localDisturbancePos = message.localDisturbancePos;
		}
	}
}
