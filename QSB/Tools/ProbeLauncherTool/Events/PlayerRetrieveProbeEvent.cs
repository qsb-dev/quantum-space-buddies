using QSB.Events;
using QSB.Messaging;
using QSB.Player;

namespace QSB.Tools.ProbeLauncherTool.Events
{
	internal class PlayerRetrieveProbeEvent : QSBEvent<BoolMessage>
	{
		public override void SetupListener()
			=> GlobalMessenger<bool>.AddListener(EventNames.QSBPlayerRetrieveProbe, Handler);

		public override void CloseListener()
			=> GlobalMessenger<bool>.RemoveListener(EventNames.QSBPlayerRetrieveProbe, Handler);

		private void Handler(bool playEffects) => SendEvent(CreateMessage(playEffects));

		private BoolMessage CreateMessage(bool playEffects) => new()
		{
			AboutId = LocalPlayerId,
			Value = playEffects
		};

		public override void OnReceiveRemote(bool server, BoolMessage message)
		{
			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			player.ProbeLauncher.RetrieveProbe(message.Value);
		}
	}
}
