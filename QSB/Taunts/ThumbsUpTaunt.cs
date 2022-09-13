using QSB.Taunts.ThirdPersonCamera;

namespace QSB.Taunts;

internal class ThumbsUpTaunt : ITaunt
{
	public bool Loops => false;
	public TauntBodyGroup BodyGroup => TauntBodyGroup.RightArm;
	public string TriggerName => "ThumbsUp";
	public string ClipName => "Thumbs Up";
	public string StateName => "Thumbs Up";
	public CameraMode CameraMode => CameraMode.ThirdPerson;
}
