using QSB.Taunts.ThirdPersonCamera;

namespace QSB.Taunts;

internal class DefaultDanceTaunt : ITaunt
{
	public bool Loops => false;
	public TauntBodyGroup BodyGroup => TauntBodyGroup.WholeBody;
	public string TriggerName => "DefaultDance";
	public CameraMode CameraMode => CameraMode.ThirdPerson;
	public float EnableCancelTime => -1;
}
