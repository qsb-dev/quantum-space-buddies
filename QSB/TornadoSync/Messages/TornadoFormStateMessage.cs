using QSB.Messaging;
using QSB.TornadoSync.WorldObjects;

namespace QSB.TornadoSync.Messages;

public class TornadoFormStateMessage : QSBWorldObjectMessage<QSBTornado, bool>
{
	public TornadoFormStateMessage(bool formState) => Value = formState;

	public override void OnReceiveRemote() => WorldObject.FormState = Value;
}