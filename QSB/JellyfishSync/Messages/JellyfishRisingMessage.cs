using QSB.JellyfishSync.WorldObjects;
using QSB.Messaging;

namespace QSB.JellyfishSync.Messages;

public class JellyfishRisingMessage : QSBWorldObjectMessage<QSBJellyfish, bool>
{
	public JellyfishRisingMessage(bool isRising) : base(isRising) { }

	public override void OnReceiveRemote() => WorldObject.SetIsRising(Data);
}