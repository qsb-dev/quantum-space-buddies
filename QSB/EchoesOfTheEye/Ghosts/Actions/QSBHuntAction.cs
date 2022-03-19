using System;
using System.Collections.Generic;
using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
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

	private List<int> _spotlightIndexList = new List<int>(16);

	private int _spotlightIndex = -1;

	private Vector3 _DEBUG_localPos;

	private Vector3 _DEBUG_localVel;

	public override void Initialize(QSBGhostBrain brain)
	{
		base.Initialize(brain);
		this._numNodesToSearch = 0;
		this._nodesToSearch = new GhostNodeMap.NodeSearchData[this._controller.GetNodeMap().GetNodeCount()];
		this._currentNodeIndex = 0;
		this._huntStarted = false;
		this._huntStartTime = 0f;
		this._huntFailed = false;
		this._huntFailTime = 0f;
		_controller.OnNodeMapChanged += new OWEvent.OWCallback(this.OnNodeMapChanged);
	}

	private void OnNodeMapChanged()
	{
		if (this._running)
		{
			Debug.LogError("Changing node maps while the Hunt action is running is almost definitely not supported!");
			this._huntFailed = true;
		}
		this._numNodesToSearch = 0;
		this._nodesToSearch = new GhostNodeMap.NodeSearchData[this._controller.GetNodeMap().GetNodeCount()];
		this._currentNodeIndex = 0;
	}

	public override GhostAction.Name GetName()
	{
		return GhostAction.Name.Hunt;
	}

	public override float CalculateUtility()
	{
		if (this._data.threatAwareness < GhostData.ThreatAwareness.IntruderConfirmed)
		{
			return -100f;
		}
		if (this._huntFailed && this._huntFailTime > this._data.timeLastSawPlayer)
		{
			return -100f;
		}
		if (this._running || this._data.timeSincePlayerLocationKnown < 60f)
		{
			return 80f;
		}
		return -100f;
	}

	protected override void OnEnterAction()
	{
		this._controller.SetLanternConcealed(true, true);
		this._controller.FaceVelocity();
		this._effects.AttachedObject.SetMovementStyle(GhostEffects.MovementStyle.Normal);
		if (!this._huntStarted || this._data.timeLastSawPlayer > this._huntStartTime)
		{
			Vector3 vector = this._data.lastKnownSensor.knowsPlayerVelocity ? this._data.lastKnownPlayerLocation.localVelocity : Vector3.zero;
			this._numNodesToSearch = this._controller.GetNodeMap().FindPossiblePlayerNodes(this._data.lastKnownPlayerLocation.localPosition, vector, 30f, this._nodesToSearch, true, null, null, null);
			this._currentNodeIndex = 0;
			this._startAtClosestNode = false;
			this._closestNode = null;
			this._huntStarted = true;
			this._huntStartTime = Time.time;
			this._huntFailed = false;
			if (this._numNodesToSearch == 0)
			{
				Debug.LogError("Failed to find nodes to hunt player!", this._controller);
				this._huntFailed = true;
				this._huntFailTime = Time.time;
			}
			this._DEBUG_localPos = this._data.lastKnownPlayerLocation.localPosition;
			this._DEBUG_localVel = vector;
		}
		if (!this._huntFailed)
		{
			this._closestNode = this._controller.GetNodeMap().FindClosestNode(this._controller.GetLocalFeetPosition());
			for (int i = 0; i < this._closestNode.visibleNodes.Count; i++)
			{
				for (int j = 0; j < this._numNodesToSearch; j++)
				{
					if (this._closestNode.visibleNodes[i] == this._nodesToSearch[j].node.index)
					{
						this._startAtClosestNode = true;
						break;
					}
				}
			}
			if (this._startAtClosestNode)
			{
				this._controller.PathfindToNode(this._closestNode, MoveType.SEARCH);
			}
			else
			{
				this._controller.PathfindToNode(this._nodesToSearch[this._currentNodeIndex].node, MoveType.SEARCH);
			}
			this._effects.AttachedObject.PlayVoiceAudioNear(global::AudioType.Ghost_Hunt, 1f);
		}
	}

	protected override void OnExitAction()
	{
		if (this._huntFailed && !this._data.isPlayerLocationKnown)
		{
			this._effects.AttachedObject.PlayVoiceAudioNear(global::AudioType.Ghost_HuntFail, 1f);
		}
	}

	public override bool Update_Action()
	{
		return !this._huntFailed && !this._data.isPlayerLocationKnown;
	}

	public override void FixedUpdate_Action()
	{
		if (this._huntStarted && !this._huntFailed && this._spotlightIndexList.Count > 0 && !this._controller.GetDreamLanternController().IsConcealed())
		{
			for (int i = 0; i < this._spotlightIndexList.Count; i++)
			{
				if (!this._nodesToSearch[this._spotlightIndexList[i]].searched)
				{
					Vector3 from = this._nodesToSearch[this._spotlightIndexList[i]].node.localPosition - this._controller.GetLocalFeetPosition();
					OWLight2 light = this._controller.GetDreamLanternController().GetLight();
					Vector3 to = this._controller.WorldToLocalDirection(light.transform.forward);
					if (Vector3.Angle(from, to) < light.GetLight().spotAngle * 0.5f - 5f && from.sqrMagnitude < light.range * light.range)
					{
						this._nodesToSearch[this._spotlightIndexList[i]].searched = true;
					}
				}
			}
		}
	}

	public override void OnTraversePathNode(GhostNode node)
	{
		for (int i = 0; i < this._numNodesToSearch; i++)
		{
			if (node == this._nodesToSearch[i].node)
			{
				this._nodesToSearch[i].searched = true;
			}
		}
	}

	public override void OnArriveAtPosition()
	{
		GhostNode node;
		if (this._startAtClosestNode)
		{
			this._startAtClosestNode = false;
			node = this._closestNode;
			for (int i = 0; i < this._numNodesToSearch; i++)
			{
				if (this._closestNode == this._nodesToSearch[i].node)
				{
					this._nodesToSearch[i].searched = true;
					break;
				}
			}
		}
		else
		{
			node = this._nodesToSearch[this._currentNodeIndex].node;
			this._nodesToSearch[this._currentNodeIndex].searched = true;
		}
		this.GenerateSpotlightList(node);
		if (this._spotlightIndexList.Count > 0)
		{
			this._controller.SetLanternConcealed(false, true);
			this.SpotlightNextNode();
			return;
		}
		this.TryContinueSearch();
	}

	public override void OnFaceNode(GhostNode node)
	{
		int num = this._spotlightIndexList[this._spotlightIndex];
		if (node != this._nodesToSearch[num].node)
		{
			Debug.LogError("Why are we facing this node??? " + node.name);
			Debug.Break();
			return;
		}
		this._nodesToSearch[num].searched = true;
		for (int i = this._spotlightIndexList.Count - 1; i >= 0; i--)
		{
			if (this._nodesToSearch[this._spotlightIndexList[i]].searched)
			{
				this._spotlightIndexList.RemoveAt(i);
			}
		}
		if (this._spotlightIndexList.Count > 0)
		{
			this.SpotlightNextNode();
			return;
		}
		this._controller.SetLanternConcealed(true, true);
		this._controller.FaceVelocity();
		this.TryContinueSearch();
	}

	private void SpotlightNextNode()
	{
		this._spotlightIndex = 0;
		int num = this._spotlightIndexList[this._spotlightIndex];
		this._controller.FaceNode(this._nodesToSearch[num].node, TurnSpeed.MEDIUM, 1f, true);
	}

	private void TryContinueSearch()
	{
		if (Time.time > this._enterTime + 60f)
		{
			this._huntFailed = true;
			this._huntFailTime = Time.time;
			return;
		}
		while (this._nodesToSearch[this._currentNodeIndex].searched && this._currentNodeIndex < this._numNodesToSearch)
		{
			this._currentNodeIndex++;
		}
		if (this._currentNodeIndex < this._numNodesToSearch)
		{
			this._controller.PathfindToNode(this._nodesToSearch[this._currentNodeIndex].node, MoveType.SEARCH);
			return;
		}
		this._huntFailed = true;
		this._huntFailTime = Time.time;
	}

	private void GenerateSpotlightList(GhostNode node)
	{
		this._spotlightIndexList.Clear();
		for (int i = 0; i < node.visibleNodes.Count; i++)
		{
			for (int j = 0; j < this._numNodesToSearch; j++)
			{
				if (!this._nodesToSearch[j].searched && node.visibleNodes[i] == this._nodesToSearch[j].node.index)
				{
					this._spotlightIndexList.Add(j);
				}
			}
		}
	}

	public override void DrawGizmos(bool isGhostSelected)
	{
		if (isGhostSelected)
		{
			Popcron.Gizmos.Cube(_controller.transform.position + _DEBUG_localPos, Quaternion.identity, Vector3.one, Color.white);
			Popcron.Gizmos.Line(_controller.transform.position + _DEBUG_localPos, _controller.transform.position + _DEBUG_localPos + _DEBUG_localVel, Color.white);

			for (int i = 0; i < this._numNodesToSearch; i++)
			{
				float t = Mathf.Abs(this._nodesToSearch[i].score) / 180f;
				Popcron.Gizmos.Sphere(
					_controller.LocalToWorldPosition(this._nodesToSearch[i].node.localPosition),
					(i < this._currentNodeIndex) ? 0.5f : 2f,
					this._nodesToSearch[i].searched ? Color.black : Color.HSVToRGB(Mathf.Lerp(0.5f, 0f, t), 1f, 1f));
			}
		}
	}
}
