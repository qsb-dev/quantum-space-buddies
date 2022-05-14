using GhostEnums;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Actions;

public class QSBSearchAction : QSBGhostAction
{
	private GhostNode _targetSearchNode;

	private bool _searchingAtNode;

	private float _searchStartTime;

	public override GhostAction.Name GetName()
	{
		return GhostAction.Name.SearchForIntruder;
	}

	public override float CalculateUtility()
	{
		if (!_controller.AttachedObject.GetNodeMap().HasSearchNodes(_controller.AttachedObject.GetNodeLayer()))
		{
			return -100f;
		}

		if (_data.threatAwareness >= GhostData.ThreatAwareness.SomeoneIsInHere)
		{
			return 50f;
		}

		return -100f;
	}

	protected override void OnEnterAction()
	{
		_controller.SetLanternConcealed(true, true);
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Normal);
		ContinueSearch();
	}

	protected override void OnExitAction()
	{
		if (_searchingAtNode)
		{
			_controller.FaceVelocity();
		}

		_targetSearchNode = null;
		_searchingAtNode = false;
	}

	public override bool Update_Action()
	{
		if (_searchingAtNode && Time.time > _searchStartTime + 4f)
		{
			_controller.FaceVelocity();
			_targetSearchNode.searchData.lastSearchTime = Time.time;
			ContinueSearch();
		}

		return true;
	}

	public override void OnArriveAtPosition()
	{
		_controller.Spin(TurnSpeed.MEDIUM);
		_searchingAtNode = true;
		_searchStartTime = Time.time;
	}

	private void ContinueSearch()
	{
		_searchingAtNode = false;
		_targetSearchNode = GetHighestPriorityNodeToSearch();
		if (_targetSearchNode == null)
		{
			Debug.LogError("Failed to find any nodes to search!  Did we exhaust our existing options?", _controller.AttachedObject);
			Debug.Break();
		}

		_controller.PathfindToNode(_targetSearchNode, MoveType.SEARCH);
		_controller.FaceVelocity();
	}

	private GhostNode GetHighestPriorityNodeToSearch()
	{
		var searchNodesOnLayer = _controller.AttachedObject.GetNodeMap().GetSearchNodesOnLayer(_controller.AttachedObject.GetNodeLayer());
		var num = 0f;
		var time = Time.time;
		for (var i = 0; i < searchNodesOnLayer.Length; i++)
		{
			var num2 = time - searchNodesOnLayer[i].searchData.lastSearchTime;
			num += num2;
		}

		num /= (float)searchNodesOnLayer.Length;
		GhostNode ghostNode = null;
		for (var j = 0; j < 5; j++)
		{
			ghostNode = searchNodesOnLayer[Random.Range(0, searchNodesOnLayer.Length)];
			if (time - ghostNode.searchData.lastSearchTime > num)
			{
				break;
			}
		}

		return ghostNode;
	}
}
