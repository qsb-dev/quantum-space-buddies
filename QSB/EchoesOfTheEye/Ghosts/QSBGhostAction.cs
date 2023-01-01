using QSB.EchoesOfTheEye.Ghosts.Actions;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts;

public abstract class QSBGhostAction
{
	protected QSBGhostBrain _brain;
	protected QSBGhostData _data => _brain._data;
	protected QSBGhostController _controller => _brain.AttachedObject._controller.GetWorldObject<QSBGhostController>();
	protected QSBGhostSensors _sensors => _brain.AttachedObject._sensors.GetWorldObject<QSBGhostSensors>();
	protected QSBGhostEffects _effects => _brain.AttachedObject._effects.GetWorldObject<QSBGhostEffects>();
	protected Transform _transform;
	protected bool _running;
	protected float _enterTime;

	public static QSBGhostAction CreateAction(GhostAction.Name name)
	{
		if (!QSBCore.IsHost)
		{
			return new QSBGhostActionStub { Name = name };
		}

		QSBGhostAction ghostAction;
		switch (name)
		{
			case GhostAction.Name.Wait:
				ghostAction = new QSBWaitAction();
				break;
			case GhostAction.Name.Sleep:
				ghostAction = new QSBSleepAction();
				break;
			case GhostAction.Name.Sleepwalk:
				ghostAction = new QSBSleepwalkAction();
				break;
			case GhostAction.Name.PartyPath:
				ghostAction = new QSBPartyPathAction();
				break;
			case GhostAction.Name.PartyHouse:
				ghostAction = new QSBPartyHouseAction();
				break;
			case GhostAction.Name.ElevatorWalk:
				ghostAction = new QSBElevatorWalkAction();
				break;
			case GhostAction.Name.Sentry:
				ghostAction = new QSBSentryAction();
				break;
			case GhostAction.Name.Guard:
				ghostAction = new QSBGuardAction();
				break;
			case GhostAction.Name.IdentifyIntruder:
				ghostAction = new QSBIdentifyIntruderAction();
				break;
			case GhostAction.Name.Chase:
				ghostAction = new QSBChaseAction();
				break;
			case GhostAction.Name.Hunt:
				ghostAction = new QSBHuntAction();
				break;
			case GhostAction.Name.Stalk:
				ghostAction = new QSBStalkAction();
				break;
			case GhostAction.Name.Grab:
				ghostAction = new QSBGrabAction();
				break;
			case GhostAction.Name.SearchForIntruder:
				ghostAction = new QSBSearchAction();
				break;
			default:
				Debug.LogError("Failed to create action from name " + name);
				return null;
		}

		if (ghostAction.GetName() != name)
		{
			Debug.LogError("New action name " + ghostAction.GetName() + "does not match supplied name " + name);
			Debug.Break();
		}

		return ghostAction;
	}

	public virtual void Initialize(QSBGhostBrain brain)
	{
		_brain = brain;
		this._transform = this._controller.AttachedObject.transform;
	}

	public void EnterAction()
	{
		this._running = true;
		this._enterTime = Time.time;

		if (!QSBCore.IsHost)
		{
			return;
		}

		this.OnEnterAction();
	}

	public void ExitAction()
	{
		this._running = false;

		if (!QSBCore.IsHost)
		{
			return;
		}

		this.OnExitAction();
	}

	public abstract GhostAction.Name GetName();

	public abstract float CalculateUtility();

	public abstract bool Update_Action();

	public virtual bool IsInterruptible()
	{
		return true;
	}

	public virtual float GetActionDelay()
	{
		return 0f;
	}

	public virtual void FixedUpdate_Action()
	{
	}

	public virtual void OnArriveAtPosition()
	{
	}

	public virtual void OnTraversePathNode(GhostNode node)
	{
	}

	public virtual void OnFaceNode(GhostNode node)
	{
	}

	public virtual void OnFinishFaceNodeList()
	{
	}

	public virtual void DrawGizmos(bool isGhostSelected)
	{
	}

	public virtual void OnSetAsPending()
	{
	}

	protected virtual void OnEnterAction()
	{
	}

	protected virtual void OnExitAction()
	{
	}

	protected float GetActionTimeElapsed()
	{
		if (!this._running)
		{
			return -1f;
		}

		return Time.time - this._enterTime;
	}
}
