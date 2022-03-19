using GhostEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Actions;

public class QSBWaitAction : QSBGhostAction
{
	public override GhostAction.Name GetName()
	{
		return GhostAction.Name.Wait;
	}

	public override float CalculateUtility()
	{
		if (PlayerState.IsGrabbedByGhost() && (this._running || this._data.timeSincePlayerLocationKnown < 4f))
		{
			return 666f;
		}
		return 0f;
	}

	protected override void OnEnterAction()
	{
		if (!PlayerState.IsGrabbedByGhost())
		{
			this._controller.StopMoving();
			this._controller.StopFacing();
			return;
		}
		this._effects.AttachedObject.SetMovementStyle(GhostEffects.MovementStyle.Stalk);
		this._controller.FacePlayer(TurnSpeed.MEDIUM);
		if (this._data.playerLocation.distanceXZ < 3f)
		{
			Vector3 toPositionXZ = this._data.playerLocation.toPositionXZ;
			this._controller.MoveToLocalPosition(this._controller.GetLocalFeetPosition() - toPositionXZ * 3f, MoveType.SEARCH);
			return;
		}
		this._controller.StopMoving();
	}

	public override bool Update_Action()
	{
		return true;
	}
}
