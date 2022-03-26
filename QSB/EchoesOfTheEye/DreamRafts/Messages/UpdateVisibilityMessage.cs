using QSB.EchoesOfTheEye.DreamRafts.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.DreamRafts.Messages;

public class UpdateVisibilityMessage : QSBWorldObjectMessage<QSBDreamRaftProjection, (bool Visible, bool Immediate)>
{
	public UpdateVisibilityMessage(bool visible, bool immediate) : base((visible, immediate)) { }

	public override void OnReceiveRemote()
	{
		if (WorldObject.AttachedObject._visible == Data.Visible)
		{
			return;
		}

		WorldObject.AttachedObject._visible = Data.Visible;
		QSBPatch.RemoteCall(() => WorldObject.AttachedObject.UpdateVisibility(Data.Immediate));
	}
}
