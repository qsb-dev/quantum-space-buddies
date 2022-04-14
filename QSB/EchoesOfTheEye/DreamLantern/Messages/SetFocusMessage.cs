using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

internal class SetFocusMessage : QSBWorldObjectMessage<QSBDreamLantern, float>
{
	public SetFocusMessage(float focus) : base(focus) { }

	public override void OnReceiveRemote()
		=> QSBPatch.RemoteCall(() => WorldObject.AttachedObject.SetFocus(Data));
}
