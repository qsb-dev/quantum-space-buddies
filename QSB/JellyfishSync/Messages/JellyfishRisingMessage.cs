using QSB.JellyfishSync.WorldObjects;
using QSB.Messaging;

namespace QSB.JellyfishSync.Messages
{
	public class JellyfishRisingMessage : QSBWorldObjectMessage<QSBJellyfish, bool>
	{
		public JellyfishRisingMessage(bool isRising) => Value = isRising;

		public override void OnReceiveRemote() => WorldObject.SetIsRising(Value);
	}
}