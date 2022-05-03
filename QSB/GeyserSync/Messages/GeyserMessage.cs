using QSB.GeyserSync.WorldObjects;
using QSB.Messaging;

namespace QSB.GeyserSync.Messages;

public class GeyserMessage : QSBWorldObjectMessage<QSBGeyser, bool>
{
	public GeyserMessage(bool state) : base(state) { }

	public override void OnReceiveRemote() => WorldObject.SetState(Data);
}