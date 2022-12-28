using QSB.Taunts.ThirdPersonCamera;

namespace QSB.Taunts;

internal class ThumbsUpTaunt : ITaunt
{
	public bool Loops => false;
	public TauntBodyGroup BodyGroup => TauntBodyGroup.RightArm;
	public string TriggerName => "ThumbsUp";
	public CameraMode CameraMode => CameraMode.FirstPerson;
	public float EnableCancelTime => -1;
}
