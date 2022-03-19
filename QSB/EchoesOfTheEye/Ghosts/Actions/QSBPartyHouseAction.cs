using System;
using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using UnityEngine;

public class QSBPartyHouseAction : QSBGhostAction
{
	private Vector3 _initialLocalPosition;

	private Vector3 _initialLocalDirection;

	private bool _allowChasePlayer;

	private bool _waitingToLookAtPlayer;

	private bool _lookingAtPlayer;

	private float _lookAtPlayerTime;

	private TurnSpeed _lookSpeed;

	public override GhostAction.Name GetName()
	{
		return GhostAction.Name.PartyHouse;
	}

	public override float CalculateUtility()
	{
		if (!this._allowChasePlayer)
		{
			return 99f;
		}
		return 94f;
	}

	public override bool IsInterruptible()
	{
		return true;
	}

	public void ResetAllowChasePlayer()
	{
		this._allowChasePlayer = false;
	}

	public void AllowChasePlayer()
	{
		this._allowChasePlayer = true;
		this._controller.SetLanternConcealed(true, true);
		this._controller.FacePlayer(TurnSpeed.MEDIUM);
		this._effects.AttachedObject.SetMovementStyle(GhostEffects.MovementStyle.Stalk);
	}

	public void LookAtPlayer(float delay, TurnSpeed lookSpeed = TurnSpeed.SLOWEST)
	{
		this._waitingToLookAtPlayer = true;
		this._lookAtPlayerTime = Time.time + delay;
		this._lookSpeed = lookSpeed;
	}

	public override void Initialize(QSBGhostBrain brain)
	{
		base.Initialize(brain);
		this._initialLocalPosition = this._controller.GetLocalFeetPosition();
		this._initialLocalDirection = this._controller.GetLocalForward();
	}

	protected override void OnEnterAction()
	{
		this._controller.MoveToLocalPosition(this._initialLocalPosition, MoveType.PATROL);
		this._controller.FaceLocalPosition(this._initialLocalPosition + this._initialLocalDirection, TurnSpeed.MEDIUM);
		this._controller.SetLanternConcealed(true, true);
		this._effects.AttachedObject.SetMovementStyle(GhostEffects.MovementStyle.Normal);
		this._waitingToLookAtPlayer = false;
		this._lookingAtPlayer = false;
	}

	protected override void OnExitAction()
	{
		this._allowChasePlayer = true;
	}

	public override bool Update_Action()
	{
		if (!this._lookingAtPlayer)
		{
			bool isIlluminatedByPlayer = this._data.sensor.isIlluminatedByPlayer;
			if ((this._waitingToLookAtPlayer && Time.time > this._lookAtPlayerTime) || isIlluminatedByPlayer)
			{
				this._controller.FacePlayer(isIlluminatedByPlayer ? TurnSpeed.SLOW : this._lookSpeed);
				this._waitingToLookAtPlayer = false;
				this._lookingAtPlayer = true;
			}
		}
		return true;
	}

	public override void FixedUpdate_Action()
	{
		if (this._allowChasePlayer)
		{
			if (this._controller.GetNodeMap().CheckLocalPointInBounds(this._data.playerLocation.localPosition))
			{
				this._controller.PathfindToLocalPosition(this._data.playerLocation.localPosition, MoveType.SEARCH);
			}
			this._controller.FaceLocalPosition(this._data.playerLocation.localPosition, TurnSpeed.MEDIUM);
		}
	}
}
