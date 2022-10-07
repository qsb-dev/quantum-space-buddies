using QSB.EchoesOfTheEye.Sarcophagus.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.Sarcophagus.Messages;

public class OpenMessage : QSBWorldObjectMessage<QSBSarcophagus>
{
	public override void OnReceiveRemote() => WorldObject.AttachedObject.OnPressInteract();
}
