using System;
using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts;
using UnityEngine;

public class QSBPartyPathAction : QSBGhostAction
{
	private int _pathIndex;

	private bool _allowFollowPath;

	private bool _reachedEndOfPath;

	private bool _isMovingToFinalPosition;

	private Vector3 _finalPosition;

	public int currentPathIndex
	{
		get
		{
			return this._pathIndex;
		}
	}

	public bool hasReachedEndOfPath
	{
		get
		{
			return this._reachedEndOfPath;
		}
	}

	public bool isMovingToFinalPosition
	{
		get
		{
			return this._isMovingToFinalPosition;
		}
	}

	public bool allowHearGhostCall
	{
		get
		{
			return this._allowFollowPath && !this._isMovingToFinalPosition;
		}
	}

	public override GhostAction.Name GetName()
	{
		return GhostAction.Name.PartyPath;
	}

	public override float CalculateUtility()
	{
		if (this._controller.AttachedObject.GetNodeMap().GetPathNodes().Length == 0)
		{
			return -100f;
		}
		return 10f;
	}

	public void ResetPath()
	{
		this._pathIndex = 0;
		this._allowFollowPath = false;
		this._reachedEndOfPath = false;
		this._isMovingToFinalPosition = false;
		this._controller.StopMoving();
		this._controller.SetLanternConcealed(true, false);
	}

	public void StartFollowPath()
	{
		this._allowFollowPath = true;
		this._controller.PathfindToNode(this._controller.AttachedObject.GetNodeMap().GetPathNodes()[this._pathIndex], MoveType.PATROL);
		this._controller.SetLanternConcealed(false, false);
	}

	public void MoveToFinalPosition(Vector3 worldPosition)
	{
		this._isMovingToFinalPosition = true;
		this._finalPosition = this._controller.AttachedObject.WorldToLocalPosition(worldPosition);
		this._controller.PathfindToLocalPosition(this._finalPosition, MoveType.PATROL);
		this._controller.SetLanternConcealed(true, true);
	}

	protected override void OnEnterAction()
	{
		this._controller.FaceVelocity();
		this._effects.PlayDefaultAnimation();
		this._effects.SetMovementStyle(GhostEffects.MovementStyle.Normal);
		if (this._allowFollowPath)
		{
			if (this._isMovingToFinalPosition)
			{
				this._controller.PathfindToLocalPosition(this._finalPosition, MoveType.PATROL);
			}
			else
			{
				this._controller.PathfindToNode(this._controller.AttachedObject.GetNodeMap().GetPathNodes()[this._pathIndex], MoveType.PATROL);
			}
			this._controller.SetLanternConcealed(this._isMovingToFinalPosition, true);
			this._controller.ChangeLanternFocus(0f, 2f);
			return;
		}
		this._controller.SetLanternConcealed(true, false);
	}

	protected override void OnExitAction()
	{
		this._reachedEndOfPath = false;
	}

	public override bool Update_Action()
	{
		return true;
	}

	public override void OnArriveAtPosition()
	{
		if (this._isMovingToFinalPosition)
		{
			return;
		}
		var pathNodes = this._controller.AttachedObject.GetNodeMap().GetPathNodes();
		if (this._pathIndex + 1 > pathNodes.Length || pathNodes[this._pathIndex].pathData.isEndOfPath)
		{
			this._reachedEndOfPath = true;
			return;
		}
		this._pathIndex++;
		this._controller.PathfindToNode(pathNodes[this._pathIndex], MoveType.PATROL);
	}
}
