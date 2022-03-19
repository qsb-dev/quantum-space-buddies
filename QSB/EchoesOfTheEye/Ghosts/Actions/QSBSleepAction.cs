using QSB.Utility;

namespace QSB.EchoesOfTheEye.Ghosts.Actions;

public class QSBSleepAction : QSBGhostAction
{
	private SleepAction.WakeState _state;

	public override GhostAction.Name GetName()
		=> GhostAction.Name.Sleep;

	public override float CalculateUtility()
		=> !_data.hasWokenUp
			? 100f
			: -100f;

	public override bool IsInterruptible()
		=> false;

	protected override void OnEnterAction()
		=> EnterSleepState();

	protected override void OnExitAction() { }

	public override bool Update_Action()
	{
		if (_state == SleepAction.WakeState.Sleeping)
		{
			if (_data.hasWokenUp || _data.sensor.isIlluminatedByPlayer)
			{
				DebugLog.DebugWrite($"{_brain.AttachedObject._name} mm that was a good sleep");
				_state = SleepAction.WakeState.Awake;
				_effects.AttachedObject.PlayDefaultAnimation();
			}
		}
		else if (_state is not SleepAction.WakeState.WakingUp and SleepAction.WakeState.Awake)
		{
			return false;
		}

		return true;
	}

	private void EnterSleepState()
	{
		_controller.SetLanternConcealed(true, true);
		_effects.AttachedObject.PlaySleepAnimation();
		_state = SleepAction.WakeState.Sleeping;
	}

	private enum WakeState
	{
		Sleeping,
		Awake
	}
}
