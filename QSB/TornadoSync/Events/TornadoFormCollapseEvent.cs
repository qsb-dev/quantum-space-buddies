using QSB.Events;
using QSB.TornadoSync.WorldObjects;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.TornadoSync.Events
{
	public class TornadoFormCollapseEvent : QSBEvent<BoolWorldObjectMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener()
			=> GlobalMessenger<QSBTornado, bool>.AddListener(EventNames.QSBJellyfishRising, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBTornado, bool>.RemoveListener(EventNames.QSBJellyfishRising, Handler);

		private void Handler(QSBTornado qsbTornado, bool formCollapse) => SendEvent(CreateMessage(qsbTornado, formCollapse));

		private BoolWorldObjectMessage CreateMessage(QSBTornado qsbTornado, bool formCollapse) => new()
		{
			ObjectId = qsbTornado.ObjectId,
			State = formCollapse
		};

		public override void OnReceiveRemote(bool isHost, BoolWorldObjectMessage message)
		{
			var qsbTornado = QSBWorldSync.GetWorldFromId<QSBTornado>(message.ObjectId);
			qsbTornado.FormCollapse(message.State);
		}
	}
}
