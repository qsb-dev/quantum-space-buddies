using System;
using System.Collections.Generic;
using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Utility;
using UnityEngine;

public class QSBHuntAction : QSBGhostAction
{
	private int _numNodesToSearch;
	private GhostNodeMap.NodeSearchData[] _nodesToSearch;
	private int _currentNodeIndex;
	private GhostNode _closestNode;
	private bool _startAtClosestNode;
	private bool _huntStarted;
	private float _huntStartTime;
	private bool _huntFailed;
	private float _huntFailTime;
	private List<int> _spotlightIndexList = new(16);
	private int _spotlightIndex = -1;

	public override void Initialize(QSBGhostBrain brain)
	{
		base.Initialize(brain);
		_numNodesToSearch = 0;
		_nodesToSearch = new GhostNodeMap.NodeSearchData[_controller.AttachedObject.GetNodeMap().GetNodeCount()];
		_currentNodeIndex = 0;
		_huntStarted = false;
		_huntStartTime = 0f;
		_huntFailed = false;
		_huntFailTime = 0f;
		_controller.AttachedObject.OnNodeMapChanged += new OWEvent.OWCallback(OnNodeMapChanged);
	}

	private void OnNodeMapChanged()
	{
		if (_running)
		{
			Debug.LogError("Changing node maps while the Hunt action is running is almost definitely not supported!");
			_huntFailed = true;
		}

		_numNodesToSearch = 0;
		_nodesToSearch = new GhostNodeMap.NodeSearchData[_controller.AttachedObject.GetNodeMap().GetNodeCount()];
		_currentNodeIndex = 0;
	}

	public override GhostAction.Name GetName()
	{
		return GhostAction.Name.Hunt;
	}

	public override float CalculateUtility()
	{
		if (_data.interestedPlayer == null)
		{
			return -100f;
		}

		if (_data.threatAwareness < GhostData.ThreatAwareness.IntruderConfirmed)
		{
			return -100f;
		}

		if (_huntFailed && _huntFailTime > _data.interestedPlayer.timeLastSawPlayer)
		{
			return -100f;
		}

		if (_running || _data.interestedPlayer.timeSincePlayerLocationKnown < 60f)
		{
			return 80f;
		}

		return -100f;
	}

	protected override void OnEnterAction()
	{
		_controller.SetLanternConcealed(true, true);
		_controller.FaceVelocity();
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Normal);
		if (!_huntStarted || _data.interestedPlayer.timeLastSawPlayer > _huntStartTime)
		{
			var knownPlayerVelocity = _data.interestedPlayer.lastKnownSensor.knowsPlayerVelocity ? _data.interestedPlayer.lastKnownPlayerLocation.localVelocity : Vector3.zero;
			_numNodesToSearch = _controller.AttachedObject.GetNodeMap().FindPossiblePlayerNodes(_data.interestedPlayer.lastKnownPlayerLocation.localPosition, knownPlayerVelocity, 30f, _nodesToSearch, true, null, null, null);
			_currentNodeIndex = 0;
			_startAtClosestNode = false;
			_closestNode = null;
			_huntStarted = true;
			_huntStartTime = Time.time;
			_huntFailed = false;
			if (_numNodesToSearch == 0)
			{
				DebugLog.DebugWrite($"{_brain.AttachedObject._name} : Failed to find nodes to hunt player.", OWML.Common.MessageType.Error);
				_huntFailed = true;
				_huntFailTime = Time.time;
			}
		}

		if (!_huntFailed)
		{
			_closestNode = _controller.AttachedObject.GetNodeMap().FindClosestNode(_controller.AttachedObject.GetLocalFeetPosition());
			for (var i = 0; i < _closestNode.visibleNodes.Count; i++)
			{
				for (var j = 0; j < _numNodesToSearch; j++)
				{
					if (_closestNode.visibleNodes[i] == _nodesToSearch[j].node.index)
					{
						_startAtClosestNode = true;
						break;
					}
				}
			}

			if (_startAtClosestNode)
			{
				_controller.PathfindToNode(_closestNode, MoveType.SEARCH);
			}
			else
			{
				_controller.PathfindToNode(_nodesToSearch[_currentNodeIndex].node, MoveType.SEARCH);
			}

			_effects.PlayVoiceAudioNear(AudioType.Ghost_Hunt, 1f);
		}
	}

	protected override void OnExitAction()
	{
		if (_huntFailed && !_data.interestedPlayer.isPlayerLocationKnown)
		{
			DebugLog.DebugWrite($"{_brain.AttachedObject._name} : Hunt failed. :(");
			_effects.PlayVoiceAudioNear(AudioType.Ghost_HuntFail, 1f);
		}
	}

	public override bool Update_Action()
	{
		return !_huntFailed && !_data.interestedPlayer.isPlayerLocationKnown;
	}

	public override void FixedUpdate_Action()
	{
		if (_huntStarted && !_huntFailed && _spotlightIndexList.Count > 0 && !_controller.AttachedObject.GetDreamLanternController().IsConcealed())
		{
			for (var i = 0; i < _spotlightIndexList.Count; i++)
			{
				if (!_nodesToSearch[_spotlightIndexList[i]].searched)
				{
					var from = _nodesToSearch[_spotlightIndexList[i]].node.localPosition - _controller.AttachedObject.GetLocalFeetPosition();
					var light = _controller.AttachedObject.GetDreamLanternController().GetLight();
					var to = _controller.AttachedObject.WorldToLocalDirection(light.transform.forward);
					if (Vector3.Angle(from, to) < (light.GetLight().spotAngle * 0.5f) - 5f && from.sqrMagnitude < light.range * light.range)
					{
						_nodesToSearch[_spotlightIndexList[i]].searched = true;
					}
				}
			}
		}
	}

	public override void OnTraversePathNode(GhostNode node)
	{
		for (var i = 0; i < _numNodesToSearch; i++)
		{
			if (node == _nodesToSearch[i].node)
			{
				_nodesToSearch[i].searched = true;
			}
		}
	}

	public override void OnArriveAtPosition()
	{
		GhostNode node;
		if (_startAtClosestNode)
		{
			_startAtClosestNode = false;
			node = _closestNode;
			for (var i = 0; i < _numNodesToSearch; i++)
			{
				if (_closestNode == _nodesToSearch[i].node)
				{
					_nodesToSearch[i].searched = true;
					break;
				}
			}
		}
		else
		{
			node = _nodesToSearch[_currentNodeIndex].node;
			_nodesToSearch[_currentNodeIndex].searched = true;
		}

		GenerateSpotlightList(node);
		if (_spotlightIndexList.Count > 0)
		{
			_controller.SetLanternConcealed(false, true);
			SpotlightNextNode();
			return;
		}

		TryContinueSearch();
	}

	public override void OnFaceNode(GhostNode node)
	{
		var num = _spotlightIndexList[_spotlightIndex];
		if (node != _nodesToSearch[num].node)
		{
			Debug.LogError("Why are we facing this node??? " + node.name);
			Debug.Break();
			return;
		}

		_nodesToSearch[num].searched = true;
		for (var i = _spotlightIndexList.Count - 1; i >= 0; i--)
		{
			if (_nodesToSearch[_spotlightIndexList[i]].searched)
			{
				_spotlightIndexList.RemoveAt(i);
			}
		}

		if (_spotlightIndexList.Count > 0)
		{
			SpotlightNextNode();
			return;
		}

		_controller.SetLanternConcealed(true, true);
		_controller.FaceVelocity();
		TryContinueSearch();
	}

	private void SpotlightNextNode()
	{
		_spotlightIndex = 0;
		var num = _spotlightIndexList[_spotlightIndex];
		_controller.FaceNode(_nodesToSearch[num].node, TurnSpeed.MEDIUM, 1f, true);
	}

	private void TryContinueSearch()
	{
		if (Time.time > _enterTime + 60f)
		{
			_huntFailed = true;
			_huntFailTime = Time.time;
			return;
		}

		while (_nodesToSearch[_currentNodeIndex].searched && _currentNodeIndex < _numNodesToSearch)
		{
			_currentNodeIndex++;
		}

		if (_currentNodeIndex < _numNodesToSearch)
		{
			DebugLog.DebugWrite($"{_brain.AttachedObject._name} : Moving to hunt at new node.");
			_controller.PathfindToNode(_nodesToSearch[_currentNodeIndex].node, MoveType.SEARCH);
			return;
		}

		_huntFailed = true;
		_huntFailTime = Time.time;
	}

	private void GenerateSpotlightList(GhostNode node)
	{
		_spotlightIndexList.Clear();
		for (var i = 0; i < node.visibleNodes.Count; i++)
		{
			for (var j = 0; j < _numNodesToSearch; j++)
			{
				if (!_nodesToSearch[j].searched && node.visibleNodes[i] == _nodesToSearch[j].node.index)
				{
					_spotlightIndexList.Add(j);
				}
			}
		}
	}

	public override void DrawGizmos(bool isGhostSelected)
	{
		if (isGhostSelected)
		{
			for (var i = 0; i < _numNodesToSearch; i++)
			{
				var t = Mathf.Abs(_nodesToSearch[i].score) / 180f;
				Popcron.Gizmos.Sphere(
					_controller.AttachedObject.LocalToWorldPosition(_nodesToSearch[i].node.localPosition),
					(i < _currentNodeIndex) ? 0.5f : 2f,
					_nodesToSearch[i].searched ? Color.black : Color.HSVToRGB(Mathf.Lerp(0.5f, 0f, t), 1f, 1f));
			}
		}
	}
}
