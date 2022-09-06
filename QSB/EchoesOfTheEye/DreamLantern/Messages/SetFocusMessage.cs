using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

internal class SetFocusMessage : QSBWorldObjectMessage<QSBDreamLantern, float>
{
	public SetFocusMessage(float focus) : base(focus) { }

	public override void OnReceiveRemote()
		=> WorldObject.AttachedObject.SetFocus(Data);
}
