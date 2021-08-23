using QSB.ClientServerStateSync;
using QSB.Events;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.DeathSync.Events
{
	internal class StartLoopEvent : QSBEvent<PlayerMessage>
	{
		public override EventType Type => EventType.StartLoop;

		public override void SetupListener() => GlobalMessenger.AddListener(EventNames.QSBStartLoop, Handler);
		public override void CloseListener() => GlobalMessenger.RemoveListener(EventNames.QSBStartLoop, Handler);

		private void Handler() => SendEvent(CreateMessage());

		private PlayerMessage CreateMessage() => new PlayerMessage
		{
			AboutId = LocalPlayerId
		};

		public override void OnReceiveLocal(bool server, PlayerMessage message)
			=> OnReceiveRemote(server, message);

		public override void OnReceiveRemote(bool server, PlayerMessage message)
		{
			DebugLog.DebugWrite($" ~~~ LOOP START ~~~");
			QSBEventManager.FireEvent(EventNames.QSBClientState, ClientState.AliveInSolarSystem);
		}
	}
}
