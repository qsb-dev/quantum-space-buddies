using QSB.JellyfishSync.WorldObjects;
using QSB.Messaging;

namespace QSB.JellyfishSync.Messages
{
	public class JellyfishRisingMessage : QSBBoolWorldObjectMessage<QSBJellyfish>
	{
		public JellyfishRisingMessage(bool isRising) => Value = isRising;

		public override void OnReceiveRemote() => WorldObject.IsRising = Value;

	}
}
