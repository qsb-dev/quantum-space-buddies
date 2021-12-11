using QSB.JellyfishSync.WorldObjects;
using QSB.Messaging;

namespace QSB.JellyfishSync.Events
{
	public class JellyfishRisingMessage : QSBBoolWorldObjectMessage<QSBJellyfish>
	{
		public JellyfishRisingMessage(QSBJellyfish qsbJellyfish) => Value = qsbJellyfish.IsRising;

		public JellyfishRisingMessage() { }

		public override void OnReceiveRemote() => WorldObject.IsRising = Value;
	}
}
