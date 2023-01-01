using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts;
using UnityEngine;

public class QSBPartyPathAction : QSBGhostAction
{
	private bool _allowFollowPath;
	private Vector3 _finalPosition;

	public int currentPathIndex { get; private set; }

	public bool hasReachedEndOfPath { get; private set; }

	public bool isMovingToFinalPosition { get; private set; }

	public bool allowHearGhostCall
	{
		get
		{
			return this._allowFollowPath && !this.isMovingToFinalPosition;
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
		this.currentPathIndex = 0;
		this._allowFollowPath = false;
		this.hasReachedEndOfPath = false;
		this.isMovingToFinalPosition = false;
		this._controller.StopMoving();
		this._controller.SetLanternConcealed(true, false);
	}

	public void StartFollowPath()
	{
		this._allowFollowPath = true;
		this._controller.PathfindToNode(this._controller.AttachedObject.GetNodeMap().GetPathNodes()[this.currentPathIndex], MoveType.PATROL);
		this._controller.SetLanternConcealed(false, false);
	}

	public void MoveToFinalPosition(Vector3 worldPosition)
	{
		this.isMovingToFinalPosition = true;
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
			if (this.isMovingToFinalPosition)
			{
				this._controller.PathfindToLocalPosition(this._finalPosition, MoveType.PATROL);
			}
			else
			{
				this._controller.PathfindToNode(this._controller.AttachedObject.GetNodeMap().GetPathNodes()[this.currentPathIndex], MoveType.PATROL);
			}

			this._controller.SetLanternConcealed(this.isMovingToFinalPosition, true);
			this._controller.ChangeLanternFocus(0f, 2f);
			return;
		}

		this._controller.SetLanternConcealed(true, false);
	}

	protected override void OnExitAction()
	{
		this.hasReachedEndOfPath = false;
	}

	public override bool Update_Action()
	{
		return true;
	}

	public override void OnArriveAtPosition()
	{
		if (this.isMovingToFinalPosition)
		{
			return;
		}

		var pathNodes = this._controller.AttachedObject.GetNodeMap().GetPathNodes();
		if (this.currentPathIndex + 1 > pathNodes.Length || pathNodes[this.currentPathIndex].pathData.isEndOfPath)
		{
			this.hasReachedEndOfPath = true;
			return;
		}

		this.currentPathIndex++;
		this._controller.PathfindToNode(pathNodes[this.currentPathIndex], MoveType.PATROL);
	}
}
