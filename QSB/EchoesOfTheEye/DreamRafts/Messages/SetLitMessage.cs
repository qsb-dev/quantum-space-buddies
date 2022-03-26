using QSB.EchoesOfTheEye.DreamRafts.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.DreamRafts.Messages;

public class SetLitMessage : QSBWorldObjectMessage<QSBDreamObjectProjector, bool>
{
	public SetLitMessage(bool data) : base(data) { }

	public override void OnReceiveRemote() =>
		QSBPatch.RemoteCall(() => WorldObject.AttachedObject.SetLit(Data));
}
