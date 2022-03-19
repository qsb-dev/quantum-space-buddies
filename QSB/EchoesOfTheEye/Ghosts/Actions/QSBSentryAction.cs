using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts;

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
		_spotlighting = false;
		_controller.SetLanternConcealed(true, true);
		_effects.AttachedObject.SetMovementStyle(GhostEffects.MovementStyle.Stalk);
		var searchNodesOnLayer = _controller.GetNodeMap().GetSearchNodesOnLayer(_controller.GetNodeLayer());
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
		if (_data.isPlayerLocationKnown && !_spotlighting)
		{
			_spotlighting = true;
			_controller.ChangeLanternFocus(1f, 2f);
		}

		if (_spotlighting)
		{
			if (_data.timeSincePlayerLocationKnown > 3f)
			{
				_spotlighting = false;
				_controller.SetLanternConcealed(true, true);
				_controller.FaceLocalPosition(_targetSentryNode.localPosition + _targetSentryNode.localForward * 10f, TurnSpeed.MEDIUM);
				return;
			}

			_controller.FaceLocalPosition(_data.lastKnownPlayerLocation.localPosition, TurnSpeed.FAST);
		}
	}

	public override void OnArriveAtPosition()
	{
		_controller.FaceLocalPosition(_targetSentryNode.localPosition + _targetSentryNode.localForward * 10f, TurnSpeed.MEDIUM);
	}
}
