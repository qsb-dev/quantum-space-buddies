using QSB.Events;

namespace QSB.QuantumSync.Events
{
	public class MultiStateChangeEvent : QSBEvent<MultiStateChangeMessage>
	{
		public override QSB.Events.EventType Type => QSB.Events.EventType.MultiStateChange;

		public override void SetupListener() => GlobalMessenger<int, int>.AddListener(EventNames.QSBMultiStateChange, Handler);
		public override void CloseListener() => GlobalMessenger<int, int>.RemoveListener(EventNames.QSBMultiStateChange, Handler);

		private void Handler(int objid, int stateIndex) => SendEvent(CreateMessage(objid, stateIndex));

		private MultiStateChangeMessage CreateMessage(int objid, int stateIndex) => new MultiStateChangeMessage
		{
			AboutId = LocalPlayerId,
			ObjectId = objid,
			StateIndex = stateIndex
		};

		public override void OnReceiveRemote(bool server, MultiStateChangeMessage message)
		{
			if (!QSBCore.HasWokenUp)
			{
				return;
			}

		}
	}
}