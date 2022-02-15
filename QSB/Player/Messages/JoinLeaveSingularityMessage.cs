using QSB.Messaging;

namespace QSB.Player.Messages
{
	/// <summary>
	/// sent by non-hosts only
	/// </summary>
	public class JoinLeaveSingularityMessage : QSBMessage<bool>
	{
		public JoinLeaveSingularityMessage(bool joining) => Value = joining;

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			JoinLeaveSingularity.Create(player, Value);
		}
	}
}
