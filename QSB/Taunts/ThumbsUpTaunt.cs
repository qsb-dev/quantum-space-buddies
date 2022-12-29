using QSB.Taunts.ThirdPersonCamera;

namespace QSB.Taunts;

internal class ThumbsUpTaunt : ITaunt
{
	public bool Loops => false;
	public TauntBodyGroup BodyGroup => TauntBodyGroup.RightArm;
	public string StateName => "Thumbs Up";
	public string TriggerName => "ThumbsUp";
	public CameraMode CameraMode => CameraMode.FirstPerson;
	public float EnableCancelTime => -1;

	public void StartTaunt() { }
	public void StopTaunt() { }
}
