using QSB.Events;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.Tools.ProbeLauncherTool.Messages
{
	internal class RetrieveProbeEvent : QSBEvent<BoolWorldObjectMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener()
			=> GlobalMessenger<QSBProbeLauncher, bool>.AddListener(EventNames.QSBRetrieveProbe, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBProbeLauncher, bool>.RemoveListener(EventNames.QSBRetrieveProbe, Handler);

		private void Handler(QSBProbeLauncher launcher, bool playEffects) => SendEvent(CreateMessage(launcher, playEffects));

		private BoolWorldObjectMessage CreateMessage(QSBProbeLauncher launcher, bool playEffects) => new()
		{
			AboutId = LocalPlayerId,
			ObjectId = launcher.ObjectId,
			State = playEffects
		};

		public override void OnReceiveRemote(bool server, BoolWorldObjectMessage message)
		{
			var worldObject = message.ObjectId.GetWorldObject<QSBProbeLauncher>();
			worldObject.RetrieveProbe(message.State);
		}
	}
}
