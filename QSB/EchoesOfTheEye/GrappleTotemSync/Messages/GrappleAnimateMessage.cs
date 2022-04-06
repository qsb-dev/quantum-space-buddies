using QSB.EchoesOfTheEye.GrappleTotemSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.GrappleTotemSync.Messages;

public class GrappleAnimateMessage : QSBWorldObjectMessage<QSBGrappleTotem>
{
	public override void OnReceiveRemote()
	{
		if (WorldObject.AttachedObject._totemAnimator != null)
		{
			WorldObject.AttachedObject._totemAnimator.SetTrigger("Zoom");
		}
	}
}
