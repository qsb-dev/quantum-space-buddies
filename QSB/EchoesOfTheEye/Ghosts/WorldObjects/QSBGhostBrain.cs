using Cysharp.Threading.Tasks;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.WorldObjects;

internal class QSBGhostBrain : WorldObject<GhostBrain>
{
	#region World Object Stuff

	public override void SendInitialState(uint to)
	{

	}

	public override async UniTask Init(CancellationToken ct)
	{
		Awake();
		Start();
	}

	public override bool ShouldDisplayDebug()
		=> base.ShouldDisplayDebug()
		&& QSBCore.DebugSettings.DrawGhostAI;

	public override string ReturnLabel()
	{
		var label = $"Name:{AttachedObject.ghostName}\r\nCurrent Action:{_currentAction?.GetName()}";

		return label;
	}

	public override void DisplayLines()
	{
		ControllerLines(AttachedObject._controller);
		DataLines(AttachedObject._data, AttachedObject._controller);

		if (_currentAction != null)
		{
			_currentAction.DrawGizmos(true);
		}
	}

	private void ControllerLines(GhostController controller)
	{
		Popcron.Gizmos.Sphere(controller.transform.position, 2f, Color.white);

		if (controller._followNodePath)
		{
			for (var i = controller._nodePath.Count - 1; i >= 0; i--)
			{
				Popcron.Gizmos.Sphere(controller.LocalToWorldPosition(controller._nodePath[i].localPosition), 0.25f, Color.cyan, 3);

				if (i != 0)
				{
					var hasVisited = controller._pathIndex < i;
					var color = hasVisited ? Color.white : Color.cyan;
					Popcron.Gizmos.Line(controller.LocalToWorldPosition(controller._nodePath[i].localPosition), controller.LocalToWorldPosition(controller._nodePath[i - 1].localPosition), color);
				}
			}

			if (controller._hasFinalPathPosition)
			{
				Popcron.Gizmos.Sphere(controller.LocalToWorldPosition(controller._finalPathPosition), 0.3f, Color.red, 8);
			}
		}
	}

	private void DataLines(GhostData data, GhostController controller)
	{
		Popcron.Gizmos.Line(controller.transform.position, controller.LocalToWorldPosition(data.lastKnownPlayerLocation.localPosition), Color.magenta);
		Popcron.Gizmos.Sphere(controller.LocalToWorldPosition(data.lastKnownPlayerLocation.localPosition), 1f, Color.magenta);
	}

	#endregion

	private List<QSBGhostAction> _actionLibrary = new();
	private QSBGhostAction _currentAction;
	private QSBGhostAction _pendingAction;

	public GhostAction.Name GetCurrentActionName()
	{
		if (_currentAction == null)
		{
			return GhostAction.Name.None;
		}
		return _currentAction.GetName();
	}

	public QSBGhostAction GetCurrentAction()
	{
		return this._currentAction;
	}

	public QSBGhostAction GetAction(GhostAction.Name actionName)
	{
		for (int i = 0; i < this._actionLibrary.Count; i++)
		{
			if (this._actionLibrary[i].GetName() == actionName)
			{
				return this._actionLibrary[i];
			}
		}
		return null;
	}

	public GhostData.ThreatAwareness GetThreatAwareness()
	{
		return AttachedObject._data.threatAwareness;
	}

	public GhostEffects GetEffects()
	{
		return AttachedObject._effects;
	}

	public void Awake()
	{
		DebugLog.DebugWrite($"{AttachedObject._name} Awake");
		AttachedObject._controller = AttachedObject.GetComponent<GhostController>();
		AttachedObject._sensors = AttachedObject.GetComponent<GhostSensors>();
		AttachedObject._data = new GhostData();
	}

	public void Start()
	{
		DebugLog.DebugWrite($"{AttachedObject._name} Start");
		AttachedObject.enabled = false;
		AttachedObject._controller.GetDreamLanternController().enabled = false;
		AttachedObject._controller.Initialize(AttachedObject._nodeLayer, AttachedObject._effects);
		AttachedObject._sensors.Initialize(AttachedObject._data, AttachedObject._guardVolume);
		AttachedObject._effects.Initialize(AttachedObject._controller.GetNodeRoot(), AttachedObject._controller, AttachedObject._data);
		AttachedObject._effects.OnCallForHelp += AttachedObject.OnCallForHelp;
		AttachedObject._data.reducedFrights_allowChase = AttachedObject._reducedFrights_allowChase;
		AttachedObject._controller.SetLanternConcealed(AttachedObject._startWithLanternConcealed, false);
		AttachedObject._intruderConfirmedBySelf = false;
		AttachedObject._intruderConfirmPending = false;
		AttachedObject._intruderConfirmTime = 0f;
		if (AttachedObject._chokePoint != null)
		{
			AttachedObject._data.hasChokePoint = true;
			AttachedObject._data.chokePointLocalPosition = AttachedObject._controller.WorldToLocalPosition(AttachedObject._chokePoint.position);
			AttachedObject._data.chokePointLocalFacing = AttachedObject._controller.WorldToLocalDirection(AttachedObject._chokePoint.forward);
		}

		for (var i = 0; i < AttachedObject._actions.Length; i++)
		{
			var ghostAction = QSBGhostAction.CreateAction(AttachedObject._actions[i]);
			ghostAction.Initialize(AttachedObject._data, AttachedObject._controller, AttachedObject._sensors, AttachedObject._effects);
			_actionLibrary.Add(ghostAction);
		}

		ClearPendingAction();
	}

	public void OnDestroy()
	{
		DebugLog.DebugWrite($"{AttachedObject._name} OnDestroy");
		AttachedObject._sensors.RemoveEventListeners();
		AttachedObject._controller.OnArriveAtPosition -= AttachedObject.OnArriveAtPosition;
		AttachedObject._controller.OnTraversePathNode -= AttachedObject.OnTraversePathNode;
		AttachedObject._controller.OnFaceNode -= AttachedObject.OnFaceNode;
		AttachedObject._controller.OnFinishFaceNodeList -= AttachedObject.OnFinishFaceNodeList;
		AttachedObject._effects.OnCallForHelp -= AttachedObject.OnCallForHelp;
		GlobalMessenger.RemoveListener("EnterDreamWorld", new Callback(AttachedObject.OnEnterDreamWorld));
		GlobalMessenger.RemoveListener("ExitDreamWorld", new Callback(AttachedObject.OnExitDreamWorld));
	}

	public void TabulaRasa()
	{
		DebugLog.DebugWrite($"{AttachedObject._name} Tabula Rasa");
		AttachedObject._intruderConfirmedBySelf = false;
		AttachedObject._intruderConfirmPending = false;
		AttachedObject._intruderConfirmTime = 0f;
		AttachedObject._playResponseAudio = false;
		AttachedObject._data.TabulaRasa();
	}

	public void Die()
	{
		DebugLog.DebugWrite($"{AttachedObject._name} Die");

		if (!AttachedObject._data.isAlive)
		{
			return;
		}

		AttachedObject._data.isAlive = false;
		AttachedObject._controller.StopMoving();
		AttachedObject._controller.StopFacing();
		AttachedObject._controller.ExtinguishLantern();
		AttachedObject._controller.GetCollider().GetComponent<OWCollider>().SetActivation(false);
		AttachedObject._controller.GetGrabController().ReleasePlayer();
		_pendingAction = null;
		_currentAction = null;
		AttachedObject._data.currentAction = GhostAction.Name.None;
		AttachedObject._effects.PlayDeathAnimation();
		AttachedObject._effects.PlayDeathEffects();
	}

	public void EscalateThreatAwareness(GhostData.ThreatAwareness newThreatAwareness)
	{
		DebugLog.DebugWrite($"{AttachedObject._name} Escalate threat awareness to {newThreatAwareness}");

		if (AttachedObject._data.threatAwareness < newThreatAwareness)
		{
			AttachedObject._data.threatAwareness = newThreatAwareness;
			if (AttachedObject._data.isAlive && AttachedObject._data.threatAwareness == GhostData.ThreatAwareness.IntruderConfirmed)
			{
				if (AttachedObject._intruderConfirmedBySelf)
				{
					AttachedObject._effects.PlayVoiceAudioFar(global::AudioType.Ghost_IntruderConfirmed, 1f);
					return;
				}

				if (AttachedObject._playResponseAudio)
				{
					AttachedObject._effects.PlayVoiceAudioFar(global::AudioType.Ghost_IntruderConfirmedResponse, 1f);
					AttachedObject._playResponseAudio = false;
				}
			}
		}
	}

	public void WakeUp()
	{
		DebugLog.DebugWrite($"{AttachedObject._name} Wake up");

		AttachedObject._data.hasWokenUp = true;
	}

	public bool HearGhostCall(Vector3 playerLocalPosition, float reactDelay, bool playResponseAudio = false)
	{
		DebugLog.DebugWrite($"{AttachedObject._name} Hear ghost call");

		if (AttachedObject._data.isAlive && !AttachedObject._data.hasWokenUp)
		{
			return false;
		}

		if (AttachedObject._data.threatAwareness < GhostData.ThreatAwareness.IntruderConfirmed && !AttachedObject._intruderConfirmPending)
		{
			AttachedObject._intruderConfirmedBySelf = false;
			AttachedObject._intruderConfirmPending = true;
			AttachedObject._intruderConfirmTime = Time.time + reactDelay;
			AttachedObject._playResponseAudio = playResponseAudio;
			return true;
		}

		return false;
	}

	public bool HearCallForHelp(Vector3 playerLocalPosition, float reactDelay)
	{
		DebugLog.DebugWrite($"{AttachedObject._name} hear call for help");

		if (AttachedObject._data.isAlive && !AttachedObject._data.hasWokenUp)
		{
			return false;
		}

		MonoBehaviour.print(AttachedObject._name + " responding to help call");
		if (AttachedObject._data.threatAwareness < GhostData.ThreatAwareness.IntruderConfirmed)
		{
			AttachedObject._data.threatAwareness = GhostData.ThreatAwareness.IntruderConfirmed;
			AttachedObject._intruderConfirmPending = false;
		}

		AttachedObject._effects.PlayRespondToHelpCallAudio(reactDelay);
		AttachedObject._data.reduceGuardUtility = true;
		AttachedObject._data.lastKnownPlayerLocation.UpdateLocalPosition(playerLocalPosition, AttachedObject._controller);
		AttachedObject._data.wasPlayerLocationKnown = true;
		AttachedObject._data.timeSincePlayerLocationKnown = 0f;
		return true;
	}

	public void HintPlayerLocation()
	{
		HintPlayerLocation(AttachedObject._data.playerLocation.localPosition, Time.time);
	}

	public void HintPlayerLocation(Vector3 localPosition, float informationTime)
	{
		if (!AttachedObject._data.hasWokenUp || AttachedObject._data.isPlayerLocationKnown)
		{
			return;
		}

		if (informationTime > AttachedObject._data.timeLastSawPlayer)
		{
			AttachedObject._data.lastKnownPlayerLocation.UpdateLocalPosition(localPosition, AttachedObject._controller);
			AttachedObject._data.wasPlayerLocationKnown = true;
			AttachedObject._data.timeSincePlayerLocationKnown = 0f;
		}
	}

	public void FixedUpdate()
	{
		if (!AttachedObject.enabled)
		{
			return;
		}
		AttachedObject._controller.FixedUpdate_Controller();
		AttachedObject._sensors.FixedUpdate_Sensors();
		AttachedObject._data.FixedUpdate_Data(AttachedObject._controller, AttachedObject._sensors);
		AttachedObject.FixedUpdate_ThreatAwareness();
		if (_currentAction != null)
		{
			_currentAction.FixedUpdate_Action();
		}
	}

	public void Update()
	{
		if (!AttachedObject.enabled)
		{
			return;
		}
		AttachedObject._controller.Update_Controller();
		AttachedObject._sensors.Update_Sensors();
		AttachedObject._effects.Update_Effects();
		var flag = false;
		if (_currentAction != null)
		{
			flag = _currentAction.Update_Action();
		}

		if (!flag && _currentAction != null)
		{
			_currentAction.ExitAction();
			AttachedObject._data.previousAction = _currentAction.GetName();
			_currentAction = null;
			AttachedObject._data.currentAction = GhostAction.Name.None;
		}

		if (AttachedObject._data.isAlive && !Locator.GetDreamWorldController().IsExitingDream())
		{
			AttachedObject.EvaluateActions();
		}
	}

	public void FixedUpdate_ThreatAwareness()
	{
		if (AttachedObject._data.threatAwareness == GhostData.ThreatAwareness.IntruderConfirmed)
		{
			return;
		}
		if (!AttachedObject._intruderConfirmPending && (AttachedObject._data.threatAwareness > GhostData.ThreatAwareness.EverythingIsNormal || AttachedObject._data.playerLocation.distance < 20f || AttachedObject._data.sensor.isPlayerIlluminatedByUs) && (AttachedObject._data.sensor.isPlayerVisible || AttachedObject._data.sensor.inContactWithPlayer))
		{
			AttachedObject._intruderConfirmedBySelf = true;
			AttachedObject._intruderConfirmPending = true;
			var num = Mathf.Lerp(0.1f, 1.5f, Mathf.InverseLerp(5f, 25f, AttachedObject._data.playerLocation.distance));
			AttachedObject._intruderConfirmTime = Time.time + num;
		}
		if (AttachedObject._intruderConfirmPending && Time.time > AttachedObject._intruderConfirmTime)
		{
			AttachedObject.EscalateThreatAwareness(GhostData.ThreatAwareness.IntruderConfirmed);
			AttachedObject.OnIdentifyIntruder.Invoke(AttachedObject, AttachedObject._data);
		}
	}

	public void EvaluateActions()
	{
		if (_currentAction != null && !_currentAction.IsInterruptible())
		{
			return;
		}

		var num = float.NegativeInfinity;
		QSBGhostAction actionWithHighestUtility = null;
		for (var i = 0; i < _actionLibrary.Count; i++)
		{
			var num2 = _actionLibrary[i].CalculateUtility();
			if (num2 > num)
			{
				num = num2;
				actionWithHighestUtility = _actionLibrary[i];
			}
		}

		if (actionWithHighestUtility == null)
		{
			DebugLog.ToConsole($"Error - Couldn't find action with highest utility for {AttachedObject._name}?!", OWML.Common.MessageType.Error);
			return;
		}

		var flag = false;
		if (_pendingAction == null || (actionWithHighestUtility.GetName() != _pendingAction.GetName() && num > AttachedObject._pendingActionUtility))
		{
			_pendingAction = actionWithHighestUtility;
			AttachedObject._pendingActionUtility = num;
			AttachedObject._pendingActionTimer = _pendingAction.GetActionDelay();
			flag = true;
		}

		if (_pendingAction != null && _currentAction != null && _pendingAction.GetName() == _currentAction.GetName())
		{
			ClearPendingAction();
			flag = false;
		}

		if (flag)
		{
			_pendingAction.OnSetAsPending();
		}

		if (_pendingAction != null && AttachedObject._pendingActionTimer <= 0f)
		{
			ChangeAction(_pendingAction);
		}

		if (_pendingAction != null)
		{
			AttachedObject._pendingActionTimer -= Time.deltaTime;
		}
	}

	public void ChangeAction(QSBGhostAction action)
	{
		DebugLog.DebugWrite($"{AttachedObject._name} Change action to {action?.GetName()}");

		if (_currentAction != null)
		{
			_currentAction.ExitAction();
			AttachedObject._data.previousAction = _currentAction.GetName();
		}
		else
		{
			AttachedObject._data.previousAction = GhostAction.Name.None;
		}
		_currentAction = action;
		AttachedObject._data.currentAction = ((action != null) ? action.GetName() : GhostAction.Name.None);
		if (_currentAction != null)
		{
			_currentAction.EnterAction();
			AttachedObject._data.OnEnterAction(_currentAction.GetName());
		}
		ClearPendingAction();
	}

	public void ClearPendingAction()
	{
		_pendingAction = null;
		AttachedObject._pendingActionUtility = -100f;
		AttachedObject._pendingActionTimer = 0f;
	}

	public void OnArriveAtPosition()
	{
		if (_currentAction != null)
		{
			_currentAction.OnArriveAtPosition();
		}
	}

	public void OnTraversePathNode(GhostNode node)
	{
		if (_currentAction != null)
		{
			_currentAction.OnTraversePathNode(node);
		}
	}

	public void OnFaceNode(GhostNode node)
	{
		if (_currentAction != null)
		{
			_currentAction.OnFaceNode(node);
		}
	}

	public void OnFinishFaceNodeList()
	{
		if (_currentAction != null)
		{
			_currentAction.OnFinishFaceNodeList();
		}
	}

	public void OnCallForHelp()
	{
		if (AttachedObject._helperGhosts != null)
		{
			for (var i = 0; i < AttachedObject._helperGhosts.Length; i++)
			{
				AttachedObject._helperGhosts[i].HearCallForHelp(AttachedObject._data.playerLocation.localPosition, 3f);
			}
		}
	}

	public void OnEnterDreamWorld()
	{
		AttachedObject.enabled = true;
		AttachedObject._controller.GetDreamLanternController().enabled = true;
	}

	public void OnExitDreamWorld()
	{
		AttachedObject.enabled = false;
		AttachedObject._controller.GetDreamLanternController().enabled = false;
		ChangeAction(null);
		AttachedObject._data.OnPlayerExitDreamWorld();
	}
}
