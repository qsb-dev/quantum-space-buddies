using QSB.Messaging;
using QSB.Player;
using QSB.TriggerSync.WorldObjects;

namespace QSB.TriggerSync.Messages
{
	public class TriggerMessage : QSBBoolWorldObjectMessage<IQSBTrigger>
	{
		public TriggerMessage(bool entered) => Value = entered;

		public override void OnReceiveLocal() => OnReceiveRemote();

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			if (Value)
			{
				WorldObject.Enter(player);
			}
			else
			{
				WorldObject.Exit(player);
			}
		}
	}
}
