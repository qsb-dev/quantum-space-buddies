using Cysharp.Threading.Tasks;
using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts.Messages;
using QSB.Messaging;
using QSB.Player;
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

public class QSBGhostBrain : WorldObject<GhostBrain>, IGhostObject
{
	#region World Object Stuff

	public override async UniTask Init(CancellationToken ct)
	{
		Awake();
		Start();
	}

	public override string ReturnLabel()
	{
		var label = $"Name:{AttachedObject.ghostName}" +
			$"\r\nAwareness:{AttachedObject.GetThreatAwareness()}" +
			$"\r\nCurrent action:{AttachedObject.GetCurrentActionName()}" +
			$"\r\nMoveToTargetPosition:{AttachedObject._controller._moveToTargetPosition}" +
			$"\r\nTargetSpeed:{AttachedObject._controller._targetSpeed}" +
			$"\r\nFollowNodePath:{AttachedObject._controller._followNodePath}";

		if (QSBCore.IsHost)
		{
			foreach (var action in _actionLibrary.OrderByDescending(x => x.CalculateUtility()))
			{
				label += $"\r\n{action.GetName()}:{action.CalculateUtility()}";
			}
		}

		return label;
	}

	public override void DisplayLines()
	{
		ControllerLines(AttachedObject._controller);
		DataLines(_data, AttachedObject._controller);

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

				var hasVisited = controller._pathIndex < i;
				var color = hasVisited ? Color.white : Color.cyan;

				if (i != 0)
				{
					Popcron.Gizmos.Line(controller.LocalToWorldPosition(controller._nodePath[i].localPosition), controller.LocalToWorldPosition(controller._nodePath[i - 1].localPosition), color);
				}
			}

			if (controller._hasFinalPathPosition)
			{
				Popcron.Gizmos.Sphere(controller.LocalToWorldPosition(controller._finalPathPosition), 0.3f, Color.red, 8);
			}
		}
	}

	private void DataLines(QSBGhostData data, GhostController controller)
	{
		foreach (var player in data.players.Values)
		{
			if (player.timeSincePlayerLocationKnown != float.PositiveInfinity)
			{
				Popcron.Gizmos.Line(controller.transform.position, controller.LocalToWorldPosition(player.lastKnownPlayerLocation.localPosition), Color.magenta);
				Popcron.Gizmos.Sphere(controller.LocalToWorldPosition(player.lastKnownPlayerLocation.localPosition), 1f, Color.magenta);
			}
		}
	}

	#endregion

	internal QSBGhostData _data;
	private List<QSBGhostAction> _actionLibrary = new();
	private QSBGhostAction _currentAction;
	private QSBGhostAction _pendingAction;

	public OWEvent<GhostBrain, QSBGhostData> OnIdentifyIntruder = new(4);

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
		return _currentAction;
	}

	public QSBGhostAction GetAction(GhostAction.Name actionName)
	{
		for (int i = 0; i < _actionLibrary.Count; i++)
		{
			if (_actionLibrary[i].GetName() == actionName)
			{
				return _actionLibrary[i];
			}
		}
		return null;
	}

	public GhostData.ThreatAwareness GetThreatAwareness()
	{
		return _data.threatAwareness;
	}

	public GhostEffects GetEffects()
	{
		return AttachedObject._effects;
	}

	public bool CheckDreadAudioConditions()
	{
		return _currentAction != null
			&& _data.localPlayer.playerLocation.distance < 10f
			&& _currentAction.GetName() != GhostAction.Name.Sentry
			&& _currentAction.GetName() != GhostAction.Name.Grab;
	}

	public bool CheckFearAudioConditions(bool fearAudioAlreadyPlaying)
	{
		if (_currentAction == null)
		{
			return false;
		}

		if (_data.interestedPlayer == null)
		{
			return false;
		}

		if (_data.interestedPlayer.player != QSBPlayerManager.LocalPlayer)
		{
			return false;
		}

		return fearAudioAlreadyPlaying
			? _currentAction.GetName() is GhostAction.Name.Chase or GhostAction.Name.Grab
			: _currentAction.GetName() == GhostAction.Name.Chase;
	}

	public void Awake()
	{
		AttachedObject._controller = AttachedObject.GetComponent<GhostController>();
		AttachedObject._sensors = AttachedObject.GetComponent<GhostSensors>();
		_data = new();

		AttachedObject._controller.OnArriveAtPosition -= AttachedObject.OnArriveAtPosition;
		AttachedObject._controller.OnArriveAtPosition += OnArriveAtPosition;

		AttachedObject._controller.OnTraversePathNode -= AttachedObject.OnTraversePathNode;
		AttachedObject._controller.OnTraversePathNode += OnTraversePathNode;

		AttachedObject._controller.OnFaceNode -= AttachedObject.OnFaceNode;
		AttachedObject._controller.OnFaceNode += OnFaceNode;

		AttachedObject._controller.OnFinishFaceNodeList -= AttachedObject.OnFinishFaceNodeList;
		AttachedObject._controller.OnFinishFaceNodeList += OnFinishFaceNodeList;

		if (AttachedObject._data != null)
		{
			_data.threatAwareness = AttachedObject._data.threatAwareness;
		}
	}

	public void Start()
	{
		AttachedObject.enabled = false;
		AttachedObject._controller.GetDreamLanternController().enabled = false;
		AttachedObject._controller.GetWorldObject<QSBGhostController>().Initialize(AttachedObject._nodeLayer, AttachedObject._effects.GetWorldObject<QSBGhostEffects>());
		AttachedObject._sensors.GetWorldObject<QSBGhostSensors>().Initialize(_data);
		AttachedObject._effects.GetWorldObject<QSBGhostEffects>().Initialize(AttachedObject._controller.GetNodeRoot(), AttachedObject._controller.GetWorldObject<QSBGhostController>(), _data);
		AttachedObject._effects.OnCallForHelp += AttachedObject.OnCallForHelp;
		_data.reducedFrights_allowChase = AttachedObject._reducedFrights_allowChase;
		AttachedObject._controller.SetLanternConcealed(AttachedObject._startWithLanternConcealed, false);
		AttachedObject._intruderConfirmedBySelf = false;
		AttachedObject._intruderConfirmPending = false;
		AttachedObject._intruderConfirmTime = 0f;

		for (var i = 0; i < AttachedObject._actions.Length; i++)
		{
			var ghostAction = QSBGhostAction.CreateAction(AttachedObject._actions[i]);
			ghostAction.Initialize(this);
			_actionLibrary.Add(ghostAction);
		}

		ClearPendingAction();
	}

	public void OnDestroy()
	{
		AttachedObject._sensors.RemoveEventListeners();
		AttachedObject._controller.OnArriveAtPosition -= OnArriveAtPosition;
		AttachedObject._controller.OnTraversePathNode -= OnTraversePathNode;
		AttachedObject._controller.OnFaceNode -= OnFaceNode;
		AttachedObject._controller.OnFinishFaceNodeList -= OnFinishFaceNodeList;
		AttachedObject._effects.OnCallForHelp -= OnCallForHelp;
	}

	public void TabulaRasa()
	{
		AttachedObject._intruderConfirmedBySelf = false;
		AttachedObject._intruderConfirmPending = false;
		AttachedObject._intruderConfirmTime = 0f;
		AttachedObject._playResponseAudio = false;
		_data.TabulaRasa();
	}

	public void Die()
	{
		if (!_data.isAlive)
		{
			return;
		}

		_data.isAlive = false;
		AttachedObject._controller.GetWorldObject<QSBGhostController>().StopMoving();
		AttachedObject._controller.GetWorldObject<QSBGhostController>().StopFacing();
		AttachedObject._controller.ExtinguishLantern();
		AttachedObject._controller.GetCollider().GetComponent<OWCollider>().SetActivation(false);
		AttachedObject._controller.GetGrabController().ReleasePlayer();
		_pendingAction = null;
		_currentAction = null;
		_data.currentAction = GhostAction.Name.None;
		AttachedObject._effects.PlayDeathAnimation();
		AttachedObject._effects.PlayDeathEffects();
	}

	public void EscalateThreatAwareness(GhostData.ThreatAwareness newThreatAwareness)
	{
		DebugLog.DebugWrite($"{AttachedObject._name} Escalate threat awareness to {newThreatAwareness}");

		if (_data.threatAwareness < newThreatAwareness)
		{
			_data.threatAwareness = newThreatAwareness;
			if (_data.isAlive && _data.threatAwareness == GhostData.ThreatAwareness.IntruderConfirmed)
			{
				if (AttachedObject._intruderConfirmedBySelf)
				{
					AttachedObject._effects.GetWorldObject<QSBGhostEffects>().PlayVoiceAudioFar(global::AudioType.Ghost_IntruderConfirmed, 1f);
					return;
				}

				if (AttachedObject._playResponseAudio)
				{
					AttachedObject._effects.GetWorldObject<QSBGhostEffects>().PlayVoiceAudioFar(global::AudioType.Ghost_IntruderConfirmedResponse, 1f);
					AttachedObject._playResponseAudio = false;
				}
			}
		}
	}

	public void WakeUp()
		=> _data.hasWokenUp = true;

	public bool HearGhostCall(Vector3 playerLocalPosition, float reactDelay, bool playResponseAudio = false)
	{
		if (_data.isAlive && !_data.hasWokenUp)
		{
			return false;
		}

		if (_data.threatAwareness < GhostData.ThreatAwareness.IntruderConfirmed && !AttachedObject._intruderConfirmPending)
		{
			AttachedObject._intruderConfirmedBySelf = false;
			AttachedObject._intruderConfirmPending = true;
			AttachedObject._intruderConfirmTime = Time.time + reactDelay;
			AttachedObject._playResponseAudio = playResponseAudio;
			return true;
		}

		return false;
	}

	public bool HearCallForHelp(Vector3 playerLocalPosition, float reactDelay, GhostPlayer player)
	{
		if (_data.isAlive && !_data.hasWokenUp)
		{
			return false;
		}

		if (_data.threatAwareness < GhostData.ThreatAwareness.IntruderConfirmed)
		{
			_data.threatAwareness = GhostData.ThreatAwareness.IntruderConfirmed;
			AttachedObject._intruderConfirmPending = false;
		}

		AttachedObject._effects.PlayRespondToHelpCallAudio(reactDelay);
		_data.reduceGuardUtility = true;
		player.lastKnownPlayerLocation.UpdateLocalPosition(playerLocalPosition, AttachedObject._controller);
		player.wasPlayerLocationKnown = true;
		player.timeSincePlayerLocationKnown = 0f;
		return true;
	}

	public void HintPlayerLocation(PlayerInfo player)
	{
		var ghostPlayer = _data.players[player];
		HintPlayerLocation(ghostPlayer.playerLocation.localPosition, Time.time, ghostPlayer);
	}

	public void HintPlayerLocation(Vector3 localPosition, float informationTime, GhostPlayer player)
	{
		if (!_data.hasWokenUp || player.isPlayerLocationKnown)
		{
			return;
		}

		if (informationTime > player.timeLastSawPlayer)
		{
			player.lastKnownPlayerLocation.UpdateLocalPosition(localPosition, AttachedObject._controller);
			player.wasPlayerLocationKnown = true;
			player.timeSincePlayerLocationKnown = 0f;
		}
	}

	public void FixedUpdate()
	{
		if (!AttachedObject.enabled)
		{
			DebugLog.DebugWrite($"attached object is not enabled!");
			return;
		}

		AttachedObject._controller.FixedUpdate_Controller();
		AttachedObject._sensors.FixedUpdate_Sensors();
		_data.FixedUpdate_Data(AttachedObject._controller, AttachedObject._sensors);

		if (!QSBCore.IsHost)
		{
			return;
		}

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

		if (!QSBCore.IsHost)
		{
			return;
		}

		var flag = false;
		if (_currentAction != null)
		{
			flag = _currentAction.Update_Action();
		}

		if (!flag && _currentAction != null)
		{
			_currentAction.ExitAction();
			_data.previousAction = _currentAction.GetName();
			_currentAction = null;
			_data.currentAction = GhostAction.Name.None;
		}

		// BUG: IsExitingDream happens for one frame, but still, doesn't this not evaluate actions if host is leaving dream world?
		if (_data.isAlive && !Locator.GetDreamWorldController().IsExitingDream())
		{
			AttachedObject.EvaluateActions();
		}
	}

	public void FixedUpdate_ThreatAwareness()
	{
		if (_data.threatAwareness == GhostData.ThreatAwareness.IntruderConfirmed)
		{
			return;
		}

		if (!AttachedObject._intruderConfirmPending
			&& (_data.threatAwareness > GhostData.ThreatAwareness.EverythingIsNormal || _data.players.Values.Any(x => x.playerLocation.distance < 20f) || _data.players.Values.Any(x => x.sensor.isPlayerIlluminatedByUs))
			&& (_data.players.Values.Any(x => x.sensor.isPlayerVisible) || _data.players.Values.Any(x => x.sensor.inContactWithPlayer)))
		{
			AttachedObject._intruderConfirmedBySelf = true;
			AttachedObject._intruderConfirmPending = true;
			var closestPlayer = _data.players.Values.MinBy(x => x.playerLocation.distance);
			var num = Mathf.Lerp(0.1f, 1.5f, Mathf.InverseLerp(5f, 25f, closestPlayer.playerLocation.distance));
			AttachedObject._intruderConfirmTime = Time.time + num;
		}

		if (AttachedObject._intruderConfirmPending && Time.time > AttachedObject._intruderConfirmTime)
		{
			AttachedObject.EscalateThreatAwareness(GhostData.ThreatAwareness.IntruderConfirmed);
			OnIdentifyIntruder.Invoke(AttachedObject, _data);
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

	public void ChangeAction(QSBGhostAction action, bool remote = false)
	{
		if (!remote)
		{
			if (!QSBCore.IsHost)
			{
				return;
			}

			this.SendMessage(new ChangeActionMessage(action?.GetName() ?? GhostAction.Name.None));
		}

		DebugLog.DebugWrite($"{AttachedObject._name} Change action to {action?.GetName()}");

		if (_currentAction != null)
		{
			_currentAction.ExitAction();
			_data.previousAction = _currentAction.GetName();
		}
		else
		{
			_data.previousAction = GhostAction.Name.None;
		}
		_currentAction = action;
		_data.currentAction = (action != null) ? action.GetName() : GhostAction.Name.None;
		if (_currentAction != null)
		{
			_currentAction.EnterAction();
			_data.OnEnterAction(_currentAction.GetName());
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
		if (!QSBCore.IsHost)
		{
			return;
		}

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
				AttachedObject._helperGhosts[i].GetWorldObject<QSBGhostBrain>().HearCallForHelp(_data.interestedPlayer.playerLocation.localPosition, 3f, _data.interestedPlayer);
			}
		}
	}

	public void OnEnterDreamWorld(PlayerInfo player)
	{
		AttachedObject.enabled = true;
		AttachedObject._controller.GetDreamLanternController().enabled = true;
	}

	public void OnExitDreamWorld(PlayerInfo player)
	{
		AttachedObject._controller.GetDreamLanternController().enabled = false;
		_data.OnPlayerExitDreamWorld(player);

		if (QSBPlayerManager.PlayerList.All(x => !x.InDreamWorld))
		{
			DebugLog.DebugWrite($"No one left in dreamworld - disabling ghost");
			AttachedObject.enabled = false;
			ChangeAction(null);
		}
	}
}
