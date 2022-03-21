using GhostEnums;
using QSB.Utility;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Actions;

internal class QSBCallForHelpAction : QSBGhostAction
{
	private bool _hasCalledForHelp;
	private bool _hasStartedMoving;
	private Vector3 _scanDirection;
	private bool _hasScanDirection;
	private bool _scanningRight;
	private float _scanTimer;

	public override GhostAction.Name GetName()
		=> GhostAction.Name.CallForHelp;

	public override float CalculateUtility()
	{
		if (_running || (!_hasCalledForHelp && _data.illuminatedByPlayerMeter > 4f && _data.hasChokePoint && _data.sensor.isPlayerInGuardVolume))
		{
			return 94f;
		}

		return -100f;
	}

	public override bool IsInterruptible()
		=> true;

	protected override void OnEnterAction()
	{
		DebugLog.DebugWrite($"{_brain.AttachedObject._name} : Calling for help!");
		_hasCalledForHelp = true;
		_hasStartedMoving = false;
		_controller.ChangeLanternFocus(1f, 2f);
		_controller.StopMoving();
		_effects.AttachedObject.StopAllVoiceAudio();
		_effects.AttachedObject.PlayCallForHelpAnimation();
	}

	protected override void OnExitAction()
		=> _hasScanDirection = false;

	public override bool Update_Action()
		=> _data.sensor.isPlayerInGuardVolume;

	public override void FixedUpdate_Action()
	{
		if (!_hasStartedMoving && Time.time > _enterTime + 2f)
		{
			_controller.PathfindToLocalPosition(_data.chokePointLocalPosition, MoveType.SEARCH);
			_hasStartedMoving = true;
		}

		if (_data.timeSincePlayerLocationKnown < 4f)
		{
			_controller.FaceLocalPosition(_data.lastKnownPlayerLocation.localPosition, TurnSpeed.MEDIUM);
			_hasScanDirection = false;
			return;
		}

		if (!_hasScanDirection)
		{
			_hasScanDirection = true;
			_scanTimer = Random.Range(1f, 2f);
			_scanningRight = !_scanningRight;
			var rotation = Quaternion.AngleAxis(_scanningRight ? 30f : -30f, Vector3.up);
			_scanDirection = rotation * _data.chokePointLocalFacing;
			_controller.FaceLocalDirection(_scanDirection, TurnSpeed.SLOWEST);
			return;
		}

		if (Vector3.Angle(_scanDirection, _controller.GetLocalForward()) < 5f)
		{
			_scanTimer -= Time.deltaTime;
			if (_scanTimer < 0f)
			{
				_hasScanDirection = false;
			}
		}
	}

	public override void OnArriveAtPosition()
	{
	}
}
