using QSB.Anglerfish.WorldObjects;
using QSB.Events;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.Anglerfish.Events
{
	public class AnglerChangeStateEvent : QSBEvent<EnumWorldObjectMessage<AnglerfishController.AnglerState>>
	{
		public override EventType Type => EventType.AnglerChangeState;
		public override void SetupListener() => GlobalMessenger<int, AnglerfishController.AnglerState>.AddListener(EventNames.QSBAnglerChangeState, Handler);
		public override void CloseListener() => GlobalMessenger<int, AnglerfishController.AnglerState>.RemoveListener(EventNames.QSBAnglerChangeState, Handler);
		private void Handler(int id, AnglerfishController.AnglerState state) =>
			SendEvent(new EnumWorldObjectMessage<AnglerfishController.AnglerState>
			{
				OnlySendToHost = true,
				ObjectId = id,
				EnumValue = state
			});

		public override void OnReceiveLocal(bool isHost, EnumWorldObjectMessage<AnglerfishController.AnglerState> message)
		{
			if (message.EnumValue == AnglerfishController.AnglerState.Chasing)
			{
				QSBWorldSync.GetWorldFromId<QSBAngler>(message.ObjectId).TransferAuthority(message.FromId);
			}
		}
		public override void OnReceiveRemote(bool isHost, EnumWorldObjectMessage<AnglerfishController.AnglerState> message)
		{
			var qsbAngler = QSBWorldSync.GetWorldFromId<QSBAngler>(message.ObjectId);
			qsbAngler.AttachedObject._currentState = message.EnumValue;

			if (message.EnumValue == AnglerfishController.AnglerState.Chasing)
			{
				qsbAngler.TransferAuthority(message.FromId);
			}
		}
	}
}
