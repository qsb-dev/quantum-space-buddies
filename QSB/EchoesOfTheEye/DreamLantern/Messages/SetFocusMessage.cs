using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

public class SetFocusMessage : QSBWorldObjectMessage<QSBDreamLanternController, float>
{
	public SetFocusMessage(float focus) : base(focus) { }

	public override void OnReceiveRemote()
		=> WorldObject.AttachedObject.SetFocus(Data);
}
