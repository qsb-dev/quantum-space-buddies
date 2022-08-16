using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts;
using QSB.Utility;
using System.Linq;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class QSBGuardAction : QSBGhostAction
{
	private GhostNode _targetSearchNode;

	private bool _searchingAtNode;

	private bool _watchingPlayer;

	private float _searchStartTime;

	private float _lastSawPlayer;

	private GhostNode[] _searchNodes;

	private bool _hasReachedAnySearchNode;

	public override GhostAction.Name GetName()
	{
		return GhostAction.Name.Guard;
	}

	public override float CalculateUtility()
	{
		if (!_controller.AttachedObject.GetNodeMap().HasSearchNodes(_controller.AttachedObject.GetNodeLayer()))
		{
			return -100f;
		}

		if (_data.threatAwareness < GhostData.ThreatAwareness.SomeoneIsInHere)
		{
			return -100f;
		}

		if (_data.reduceGuardUtility)
		{
			return 60f;
		}

		return 90f;
	}

	protected override void OnEnterAction()
	{
		_controller.SetLanternConcealed(true, true);
		_sensors.AttachedObject.SetContactEdgeCatcherWidth(5f);
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Normal);
		ContinueSearch();
	}

	protected override void OnExitAction()
	{
		if (_searchingAtNode)
		{
			_controller.FaceVelocity();
		}

		_sensors.AttachedObject.ResetContactEdgeCatcherWidth();
		_targetSearchNode = null;
		_searchingAtNode = false;
		_watchingPlayer = false;
	}

	public override bool Update_Action()
	{
		if (_searchingAtNode && Time.time > _searchStartTime + 4f)
		{
			_controller.FaceVelocity();
			_targetSearchNode.searchData.lastSearchTime = Time.time;
			ContinueSearch();
		}

		var anyPlayerVisible = _data.players.Values.Any(x => x.sensor.isPlayerVisible);
		var anyPlayerLanternVisible = _data.players.Values.Any(x => x.sensor.isPlayerHeldLanternVisible);

		var flag = _hasReachedAnySearchNode && (anyPlayerVisible || anyPlayerLanternVisible);
		if (flag)
		{
			_lastSawPlayer = Time.time;
		}

		if (!_watchingPlayer && flag)
		{
			_watchingPlayer = true;
			_searchingAtNode = false;
			_controller.StopMoving();
			_controller.FacePlayer(_data.interestedPlayer.player, TurnSpeed.MEDIUM);
		}
		else if (_watchingPlayer && !flag && Time.time - _lastSawPlayer > 1f)
		{
			_watchingPlayer = false;
			ContinueSearch();
		}

		return true;
	}

	public override void OnArriveAtPosition()
	{
		if (_searchNodes != null && _searchNodes.Length == 1)
		{
			_controller.FaceLocalPosition(_targetSearchNode.localPosition + (_targetSearchNode.localForward * 10f), TurnSpeed.MEDIUM);
		}
		else
		{
			_controller.Spin(TurnSpeed.MEDIUM);
		}

		_searchingAtNode = true;
		_hasReachedAnySearchNode = true;
		_searchStartTime = Time.time;
	}

	private void ContinueSearch()
	{
		_searchingAtNode = false;
		_targetSearchNode = GetHighestPriorityNodeToSearch();
		if (_targetSearchNode == null)
		{
			DebugLog.DebugWrite($"{_brain.Name} : Failed to find any nodes to search!  Did we exhaust our existing options?", OWML.Common.MessageType.Error);
		}

		_controller.PathfindToNode(_targetSearchNode, MoveType.SEARCH);
		_controller.FaceVelocity();
	}

	private GhostNode GetHighestPriorityNodeToSearch()
	{
		_searchNodes = _controller.AttachedObject.GetNodeMap().GetSearchNodesOnLayer(_controller.AttachedObject.GetNodeLayer());
		var num = 0f;
		var time = Time.time;
		for (var i = 0; i < _searchNodes.Length; i++)
		{
			var num2 = time - _searchNodes[i].searchData.lastSearchTime;
			num += num2;
		}

		num /= _searchNodes.Length;
		GhostNode ghostNode = null;
		for (var j = 0; j < 5; j++)
		{
			ghostNode = _searchNodes[Random.Range(0, _searchNodes.Length)];
			if (time - ghostNode.searchData.lastSearchTime > num)
			{
				break;
			}
		}

		return ghostNode;
	}

	public override void DrawGizmos(bool isGhostSelected)
	{
		foreach (var item in _searchNodes)
		{
			Popcron.Gizmos.Sphere(_controller.AttachedObject.LocalToWorldPosition(item.localPosition), 1f, Color.red);
		}
	}
}
