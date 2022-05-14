using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

internal class SetLitMessage : QSBWorldObjectMessage<QSBDreamLantern, bool>
{
	public SetLitMessage(bool lit) : base(lit) { }

	public override void OnReceiveRemote()
		=> QSBPatch.RemoteCall(() => WorldObject.AttachedObject.SetLit(Data));
}
