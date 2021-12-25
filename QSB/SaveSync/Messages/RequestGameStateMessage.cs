using QSB.Messaging;

namespace QSB.SaveSync.Messages
{
	/// <summary>
	/// always sent to host
	/// </summary>
	internal class RequestGameStateMessage : QSBMessage
	{
		public RequestGameStateMessage() => To = 0;

		public override void OnReceiveRemote() => new GameStateMessage(From).Send();
	}
}
