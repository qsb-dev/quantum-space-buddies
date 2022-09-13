using QSB.Taunts.ThirdPersonCamera;

namespace QSB.Taunts;

public interface ITaunt
{
	bool Loops { get; }
	TauntBodyGroup BodyGroup { get; }
	string TriggerName { get; }
	string ClipName { get; }
	string StateName { get; }
	CameraMode CameraMode { get; }
}
