using QSB.Messaging;

namespace QSB.SaveSync.Events
{
	// always sent to host
	internal class RequestGameStateMessage : QSBMessage
	{
		public RequestGameStateMessage() => To = 0;

		public override void OnReceiveRemote() => new GameStateMessage(From).Send();
	}
}
