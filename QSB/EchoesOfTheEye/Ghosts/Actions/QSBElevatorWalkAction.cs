using GhostEnums;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Actions;

public class QSBElevatorWalkAction : QSBGhostAction
{
	private bool _calledToElevator;

	private bool _hasUsedElevator;

	private GhostNode _elevatorNode;

	public bool reachedEndOfPath { get; private set; }

	public override GhostAction.Name GetName()
	{
		return GhostAction.Name.ElevatorWalk;
	}

	public override float CalculateUtility()
	{
		if (this._calledToElevator && !this._hasUsedElevator && (_data.interestedPlayer == null || !this._data.interestedPlayer.isPlayerLocationKnown))
		{
			return 100f;
		}

		if (this._calledToElevator && !this._hasUsedElevator)
		{
			return 70f;
		}

		return -100f;
	}

	public void UseElevator()
	{
		this._hasUsedElevator = true;
	}

	public void CallToUseElevator()
	{
		this._calledToElevator = true;
		if (this._controller.AttachedObject.GetNodeMap().GetPathNodes().Length > 1)
		{
			this._elevatorNode = this._controller.AttachedObject.GetNodeMap().GetPathNodes()[1];
			this._controller.PathfindToNode(this._elevatorNode, MoveType.PATROL);
			return;
		}

		Debug.LogError("MissingElevatorNode");
	}

	protected override void OnEnterAction()
	{
		this._controller.SetLanternConcealed(true, true);
		this._controller.FaceVelocity();
		this._effects.PlayDefaultAnimation();
		this._effects.SetMovementStyle(GhostEffects.MovementStyle.Normal);
		if (this._elevatorNode != null)
		{
			this._controller.PathfindToNode(this._elevatorNode, MoveType.PATROL);
		}
	}

	protected override void OnExitAction()
	{
	}

	public override bool Update_Action()
	{
		return true;
	}

	public override void OnArriveAtPosition()
	{
		this.reachedEndOfPath = true;
	}
}
