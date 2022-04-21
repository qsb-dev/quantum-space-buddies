using System;

namespace QSB.EchoesOfTheEye.Ghosts.Actions;

public class QSBGhostActionStub : QSBGhostAction
{
	public GhostAction.Name Name;

	public override GhostAction.Name GetName()
	{
		return Name;
	}

	public override float CalculateUtility()
	{
		throw new NotImplementedException();
	}

	public override bool IsInterruptible()
	{
		throw new NotImplementedException();
	}

	protected override void OnEnterAction()
	{
		throw new NotImplementedException();
	}

	protected override void OnExitAction()
	{
		throw new NotImplementedException();
	}

	public override bool Update_Action()
	{
		throw new NotImplementedException();
	}

	public override void FixedUpdate_Action()
	{
		throw new NotImplementedException();
	}

	public override void OnArriveAtPosition()
	{
		throw new NotImplementedException();
	}
}
