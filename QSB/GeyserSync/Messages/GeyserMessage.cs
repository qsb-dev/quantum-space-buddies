using QSB.GeyserSync.WorldObjects;
using QSB.Messaging;

namespace QSB.GeyserSync.Messages
{
	public class GeyserMessage : QSBBoolWorldObjectMessage<QSBGeyser>
	{
		public GeyserMessage(bool state) => Value = state;

		public override void OnReceiveRemote() => WorldObject.SetState(Value);
	}
}