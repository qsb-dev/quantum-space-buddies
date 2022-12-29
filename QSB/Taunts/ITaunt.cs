using QSB.Taunts.ThirdPersonCamera;

namespace QSB.Taunts;

public interface ITaunt
{
	bool Loops { get; }

	TauntBodyGroup BodyGroup { get; }

	/// <summary>
	/// The name of the state containing the taunt.
	/// </summary>
	string StateName { get; }

	/// <summary>
	/// The trigger to activate the taunt.
	/// </summary>
	string TriggerName { get; }

	CameraMode CameraMode { get; }

	/// <summary>
	/// The time after the start of the taunt that the player is allowed to cancel the taunt.
	/// Set to -1 to never let this happen.
	/// </summary>
	float EnableCancelTime { get; }

	public void StartTaunt();
	public void StopTaunt();
}
