using QSB.EchoesOfTheEye.DreamRafts.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.DreamRafts.Messages;

public class SetVisibleMessage : QSBWorldObjectMessage<QSBDreamRaftProjection, bool>
{
	public SetVisibleMessage(bool data) : base(data) { }

	public override void OnReceiveRemote() =>
		QSBPatch.RemoteCall(() => WorldObject.AttachedObject.SetVisible(Data));
}
