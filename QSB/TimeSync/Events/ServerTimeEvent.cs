using QSB.Events;

namespace QSB.TimeSync.Events
{
	public class ServerTimeEvent : QSBEvent<ServerTimeMessage>
	{
		public override void SetupListener() => GlobalMessenger<float, int>.AddListener(EventNames.QSBServerTime, Handler);
		public override void CloseListener() => GlobalMessenger<float, int>.RemoveListener(EventNames.QSBServerTime, Handler);

		private void Handler(float time, int count) => SendEvent(CreateMessage(time, count));

		private ServerTimeMessage CreateMessage(float time, int count) => new()
		{
			AboutId = LocalPlayerId,
			ServerTime = time,
			LoopCount = count
		};

		public override void OnReceiveRemote(bool server, ServerTimeMessage message) =>
			WakeUpSync.LocalInstance.OnClientReceiveMessage(message);
	}
}