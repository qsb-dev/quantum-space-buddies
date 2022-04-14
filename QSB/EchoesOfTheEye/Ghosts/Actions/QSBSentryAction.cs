using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts;
using QSB.Utility;

/// <summary>
/// 
/// </summary>
public class QSBSentryAction : QSBGhostAction
{
	private GhostNode _targetSentryNode;

	private bool _spotlighting;

	public override GhostAction.Name GetName()
	{
		return GhostAction.Name.Sentry;
	}

	public override float CalculateUtility()
	{
		if (_data.threatAwareness >= GhostData.ThreatAwareness.SomeoneIsInHere)
		{
			return 50f;
		}

		return -100f;
	}

	protected override void OnEnterAction()
	{
		DebugLog.DebugWrite($"ON ENTER ACTION");
		_spotlighting = false;
		_controller.SetLanternConcealed(true, true);
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Stalk);
		var searchNodesOnLayer = _controller.AttachedObject.GetNodeMap().GetSearchNodesOnLayer(_controller.AttachedObject.GetNodeLayer());
		_targetSentryNode = searchNodesOnLayer[0];
		_controller.PathfindToNode(_targetSentryNode, MoveType.PATROL);
		_controller.FaceVelocity();
	}

	public override bool Update_Action()
	{
		return true;
	}

	public override void FixedUpdate_Action()
	{
		if (_data.interestedPlayer == null)
		{
			return;
		}

		if (_data.interestedPlayer.isPlayerLocationKnown && !_spotlighting)
		{
			DebugLog.DebugWrite($"Spotlighting player...");
			_spotlighting = true;
			_controller.ChangeLanternFocus(1f, 2f);
		}

		if (_spotlighting)
		{
			if (_data.interestedPlayer.timeSincePlayerLocationKnown > 3f)
			{
				DebugLog.DebugWrite($"Give up on spotlighting player");
				_spotlighting = false;
				_controller.SetLanternConcealed(true, true);
				_controller.FaceLocalPosition(_targetSentryNode.localPosition + _targetSentryNode.localForward * 10f, TurnSpeed.MEDIUM);
				return;
			}

			DebugLog.DebugWrite($"Facing last known position...");
			_controller.FaceLocalPosition(_data.interestedPlayer.lastKnownPlayerLocation.localPosition, TurnSpeed.FAST);
		}
	}

	public override void OnArriveAtPosition()
	{
		_controller.FaceLocalPosition(_targetSentryNode.localPosition + _targetSentryNode.localForward * 10f, TurnSpeed.MEDIUM);
	}
}
