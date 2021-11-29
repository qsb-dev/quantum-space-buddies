using QSB.Events;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.Tools.ProbeLauncherTool.Events
{
	internal class LaunchProbeEvent : QSBEvent<WorldObjectMessage>
	{
		public override void SetupListener()
			=> GlobalMessenger<QSBProbeLauncher>.AddListener(EventNames.QSBLaunchProbe, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBProbeLauncher>.RemoveListener(EventNames.QSBLaunchProbe, Handler);

		private void Handler(QSBProbeLauncher launcher) => SendEvent(CreateMessage(launcher));

		private BoolWorldObjectMessage CreateMessage(QSBProbeLauncher launcher) => new()
		{
			AboutId = LocalPlayerId,
			ObjectId = launcher.ObjectId
		};

		public override void OnReceiveRemote(bool server, WorldObjectMessage message)
		{
			var worldObject = QSBWorldSync.GetWorldFromId<QSBProbeLauncher>(message.ObjectId);
			worldObject.LaunchProbe();
		}
	}
}
