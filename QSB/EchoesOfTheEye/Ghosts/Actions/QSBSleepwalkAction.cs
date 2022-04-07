using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts;

public class QSBSleepwalkAction : QSBGhostAction
{
	public override GhostAction.Name GetName() => GhostAction.Name.Sleepwalk;

	public override float CalculateUtility()
		=> !_data.hasWokenUp
			? 100f
			: -100f;

	protected override void OnEnterAction()
	{
		MoveToRandomPatrolNode();
		_controller.SetLanternConcealed(false, true);
		_effects.AttachedObject.SetMovementStyle(GhostEffects.MovementStyle.Normal);
	}

	public override bool Update_Action()
		=> true;

	public override void OnArriveAtPosition()
		=> MoveToRandomPatrolNode();

	private void MoveToRandomPatrolNode()
	{
		_controller.PathfindToNode(_controller.AttachedObject.GetNodeMap().GetRandomPatrolNode(), MoveType.PATROL);
		_controller.FaceVelocity();
	}
}