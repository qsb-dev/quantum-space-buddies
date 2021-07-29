using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.Tools.ProbeLauncherTool.Events
{
	class PlayerRetrieveProbeEvent : QSBEvent<BoolMessage>
	{
		public override EventType Type => EventType.PlayerRetrieveProbe;

		public override void SetupListener()
			=> GlobalMessenger<bool>.AddListener(EventNames.QSBPlayerRetrieveProbe, Handler);

		public override void CloseListener()
			=> GlobalMessenger<bool>.RemoveListener(EventNames.QSBPlayerRetrieveProbe, Handler);

		private void Handler(bool playEffects) => SendEvent(CreateMessage(playEffects));

		private BoolMessage CreateMessage(bool playEffects) => new BoolMessage
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
