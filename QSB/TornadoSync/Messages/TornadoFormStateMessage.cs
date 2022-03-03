using QSB.Messaging;
using QSB.TornadoSync.WorldObjects;

namespace QSB.TornadoSync.Messages
{
	public class TornadoFormStateMessage : QSBWorldObjectMessage<QSBTornado, bool>
	{
		public TornadoFormStateMessage(bool formState) => Data = formState;

		public override void OnReceiveRemote() => WorldObject.FormState = Data;
	}
}