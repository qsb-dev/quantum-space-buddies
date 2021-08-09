using QSB.Events;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.Tools.ProbeLauncherTool.Events
{
	internal class RetrieveProbeEvent : QSBEvent<BoolWorldObjectMessage>
	{
		public override EventType Type => EventType.RetrieveProbe;

		public override void SetupListener() 
			=> GlobalMessenger<QSBProbeLauncher, bool>.AddListener(EventNames.QSBRetrieveProbe, Handler);

		public override void CloseListener() 
			=> GlobalMessenger<QSBProbeLauncher, bool>.RemoveListener(EventNames.QSBRetrieveProbe, Handler);

		private void Handler(QSBProbeLauncher launcher, bool playEffects) => SendEvent(CreateMessage(launcher, playEffects));

		private BoolWorldObjectMessage CreateMessage(QSBProbeLauncher launcher, bool playEffects) => new BoolWorldObjectMessage
		{
			AboutId = LocalPlayerId,
			ObjectId = launcher.ObjectId,
			State = playEffects
		};

		public override void OnReceiveRemote(bool server, BoolWorldObjectMessage message)
		{
			var worldObject = QSBWorldSync.GetWorldFromId<QSBProbeLauncher>(message.ObjectId);
			worldObject.RetrieveProbe(message.State);
		}
	}
}
