using QSB.GeyserSync.WorldObjects;
using QSB.Messaging;

namespace QSB.GeyserSync.Events
{
	public class GeyserMessage : QSBBoolWorldObjectMessage<QSBGeyser>
	{
		public override void OnReceiveRemote() => WorldObject.SetState(Value);
	}
}