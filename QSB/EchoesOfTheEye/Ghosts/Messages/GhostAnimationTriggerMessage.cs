using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

internal class GhostAnimationTriggerMessage : QSBWorldObjectMessage<QSBGhostEffects, GhostAnimationType>
{
	public GhostAnimationTriggerMessage(GhostAnimationType type) : base(type) { }

	public override void OnReceiveRemote()
	{
		switch (Data)
		{
			case GhostAnimationType.Sleep:
				WorldObject.PlaySleepAnimation(true);
				break;
			case GhostAnimationType.Default:
				WorldObject.PlayDefaultAnimation(true);
				break;
			case GhostAnimationType.Grab:
				WorldObject.PlayGrabAnimation(true);
				break;
			case GhostAnimationType.BlowOutLanternNormal:
				WorldObject.PlayBlowOutLanternAnimation(false, true);
				break;
			case GhostAnimationType.BlowOutLanternFast:
				WorldObject.PlayBlowOutLanternAnimation(true, true);
				break;
			case GhostAnimationType.SnapNeck:
				WorldObject.PlaySnapNeckAnimation(true);
				break;
			default:
				DebugLog.ToConsole($"Warning - Received unknown animation type of {Data} for QSBGhostEffects {WorldObject.ObjectId}", OWML.Common.MessageType.Warning);
				break;
		}
	}
}
