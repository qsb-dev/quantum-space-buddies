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
		if (_data.interestedPlayer == null)
		{
			return 0f;
		}

		if (PlayerState.IsGrabbedByGhost() && (_running || _data.interestedPlayer.timeSincePlayerLocationKnown < 4f))
		{
			return 666f;
		}

		return 0f;
	}

	protected override void OnEnterAction()
	{
		if (!PlayerState.IsGrabbedByGhost())
		{
			_controller.StopMoving();
			_controller.StopFacing();
			return;
		}

		_effects.SetMovementStyle(GhostEffects.MovementStyle.Stalk);
		_controller.FacePlayer(_data.interestedPlayer.player, TurnSpeed.MEDIUM);
		if (_data.interestedPlayer.playerLocation.distanceXZ < 3f)
		{
			var toPositionXZ = _data.interestedPlayer.playerLocation.toPositionXZ;
			_controller.MoveToLocalPosition(_controller.AttachedObject.GetLocalFeetPosition() - toPositionXZ * 3f, MoveType.SEARCH);
			return;
		}

		_controller.StopMoving();
	}

	public override bool Update_Action()
	{
		return true;
	}
}
