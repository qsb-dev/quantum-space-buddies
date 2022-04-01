using QSB.EchoesOfTheEye.Sarcophagus.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.Sarcophagus.Messages;

public class OpenMessage : QSBWorldObjectMessage<QSBSarcophagus>
{
	public override void OnReceiveRemote() =>
		QSBPatch.RemoteCall(WorldObject.AttachedObject.OnPressInteract);
}
