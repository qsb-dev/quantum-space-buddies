using QSB.Events;
using QSB.Messaging;
using QSB.Player;

namespace QSB.Tools.ProbeLauncherTool.Events
{
	internal class PlayerLaunchProbeEvent : QSBEvent<PlayerMessage>
	{
		public override bool RequireWorldObjectsReady() => true;

		public override void SetupListener()
			=> GlobalMessenger.AddListener(EventNames.QSBPlayerLaunchProbe, Handler);

		public override void CloseListener()
			=> GlobalMessenger.RemoveListener(EventNames.QSBPlayerLaunchProbe, Handler);

		private void Handler() => SendEvent(CreateMessage());

		private PlayerMessage CreateMessage() => new()
		{
			AboutId = LocalPlayerId
		};

		public override void OnReceiveRemote(bool server, PlayerMessage message)
		{
			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			player.ProbeLauncher.LaunchProbe();
		}
	}
}
