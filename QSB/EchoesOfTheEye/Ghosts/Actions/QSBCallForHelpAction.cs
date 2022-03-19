using GhostEnums;
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
	{
		return GhostAction.Name.CallForHelp;
	}

	public override float CalculateUtility()
	{
		if (this._running || (!this._hasCalledForHelp && this._data.illuminatedByPlayerMeter > 4f && this._data.hasChokePoint && this._data.sensor.isPlayerInGuardVolume))
		{
			return 94f;
		}
		return -100f;
	}

	public override bool IsInterruptible()
	{
		return true;
	}

	protected override void OnEnterAction()
	{
		this._hasCalledForHelp = true;
		this._hasStartedMoving = false;
		this._controller.ChangeLanternFocus(1f, 2f);
		this._controller.StopMoving();
		this._effects.AttachedObject.StopAllVoiceAudio();
		this._effects.AttachedObject.PlayCallForHelpAnimation();
	}

	protected override void OnExitAction()
	{
		this._hasScanDirection = false;
	}

	public override bool Update_Action()
	{
		return this._data.sensor.isPlayerInGuardVolume;
	}

	public override void FixedUpdate_Action()
	{
		if (!this._hasStartedMoving && Time.time > this._enterTime + 2f)
		{
			this._controller.PathfindToLocalPosition(this._data.chokePointLocalPosition, MoveType.SEARCH);
			this._hasStartedMoving = true;
		}
		if (this._data.timeSincePlayerLocationKnown < 4f)
		{
			this._controller.FaceLocalPosition(this._data.lastKnownPlayerLocation.localPosition, TurnSpeed.MEDIUM);
			this._hasScanDirection = false;
			return;
		}
		if (!this._hasScanDirection)
		{
			this._hasScanDirection = true;
			this._scanTimer = Random.Range(1f, 2f);
			this._scanningRight = !this._scanningRight;
			Quaternion rotation = Quaternion.AngleAxis(this._scanningRight ? 30f : -30f, Vector3.up);
			this._scanDirection = rotation * this._data.chokePointLocalFacing;
			this._controller.FaceLocalDirection(this._scanDirection, TurnSpeed.SLOWEST);
			return;
		}
		if (Vector3.Angle(this._scanDirection, this._controller.GetLocalForward()) < 5f)
		{
			this._scanTimer -= Time.deltaTime;
			if (this._scanTimer < 0f)
			{
				this._hasScanDirection = false;
			}
		}
	}

	public override void OnArriveAtPosition()
	{
	}
}
