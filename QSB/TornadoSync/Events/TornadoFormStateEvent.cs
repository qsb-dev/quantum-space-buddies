using QSB.Events;
using QSB.TornadoSync.WorldObjects;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.TornadoSync.Events
{
	public class TornadoFormStateEvent : QSBEvent<BoolWorldObjectMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener()
			=> GlobalMessenger<QSBTornado>.AddListener(EventNames.QSBTornadoFormState, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBTornado>.RemoveListener(EventNames.QSBTornadoFormState, Handler);

		private void Handler(QSBTornado qsbTornado) => SendEvent(CreateMessage(qsbTornado));

		private BoolWorldObjectMessage CreateMessage(QSBTornado qsbTornado) => new()
		{
			ObjectId = qsbTornado.ObjectId,
			State = qsbTornado.FormState
		};

		public override void OnReceiveRemote(bool isHost, BoolWorldObjectMessage message)
		{
			var qsbTornado = QSBWorldSync.GetWorldFromId<QSBTornado>(message.ObjectId);
			qsbTornado.FormState = message.State;
		}
	}
}
