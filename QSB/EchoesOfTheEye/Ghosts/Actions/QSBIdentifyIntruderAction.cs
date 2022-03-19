using System;
using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts;
using QSB.Utility;
using UnityEngine;

public class QSBIdentifyIntruderAction : QSBGhostAction
{
	private const int AGGRO_THRESHOLD = 2;

	private static GhostNode[] s_nodesToSpotlight = new GhostNode[8];

	private int _numTimesIlluminatedByPlayer;

	private bool _allowFocusBeam;

	private bool _sawPlayerOccluded;

	private bool _movingToSearchLocation;

	private bool _arrivedAtTargetSearchPosition;

	private bool _searchNodesNearTarget;

	private bool _searchNodesComplete;

	private bool _checkingTargetLocation;

	private Vector3 _searchStartPosition;

	private Vector3 _searchPosition;

	private GhostNode _searchNode;

	private float _checkTimer;

	public override GhostAction.Name GetName()
	{
		return GhostAction.Name.IdentifyIntruder;
	}

	public override float CalculateUtility()
	{
		if (_data.threatAwareness >= GhostData.ThreatAwareness.IntruderConfirmed)
		{
			return -100f;
		}

		if (_running || (_data.sensor.isPlayerHeldLanternVisible && (_data.threatAwareness > GhostData.ThreatAwareness.EverythingIsNormal || _data.playerLocation.distance < 20f)) || _data.sensor.isIlluminatedByPlayer)
		{
			return 80f;
		}

		return -100f;
	}

	public override float GetActionDelay()
	{
		if (_data.playerLocation.distance < 8f)
		{
			return 0.1f;
		}
		return 0.5f;
	}

	public override void OnSetAsPending()
	{
		if (_data.sensor.isIlluminatedByPlayer)
		{
			_numTimesIlluminatedByPlayer++;
			_allowFocusBeam = true;
		}
	}

	protected override void OnEnterAction()
	{
		DebugLog.DebugWrite($"{_brain.AttachedObject._name} OwO, who's this...?");

		_sawPlayerOccluded = false;
		_movingToSearchLocation = false;
		_arrivedAtTargetSearchPosition = false;
		_searchNodesNearTarget = false;
		_searchNodesComplete = false;
		_checkingTargetLocation = false;
		_checkTimer = 0f;
		_effects.AttachedObject.PlayVoiceAudioNear((_numTimesIlluminatedByPlayer <= 2) ? AudioType.Ghost_Identify_Curious : AudioType.Ghost_Identify_Irritated, 1f);
	}

	protected override void OnExitAction()
	{
		_controller.FaceVelocity();
		_allowFocusBeam = false;
	}

	public override bool Update_Action()
	{
		if (_checkingTargetLocation && !_data.isPlayerLocationKnown && _controller.GetSpeed() < 0.1f)
		{
			_checkTimer += Time.deltaTime;
		}
		else
		{
			_checkTimer = 0f;
		}

		if ((_searchNodesNearTarget && _checkTimer > 1f) || _checkTimer > 3f)
		{
			DebugLog.DebugWrite($"{_brain.AttachedObject._name} Couldn't identify target :(");
			_effects.AttachedObject.PlayVoiceAudioNear(AudioType.Ghost_Identify_Fail, 1f);
			return false;
		}

		return true;
	}

	public override void FixedUpdate_Action()
	{
		_checkingTargetLocation = false;
		if (!_data.wasPlayerLocationKnown && _data.isPlayerLocationKnown && _data.sensor.isIlluminatedByPlayer)
		{
			_numTimesIlluminatedByPlayer++;
		}

		if (!_allowFocusBeam && _data.sensor.isIlluminatedByPlayer)
		{
			_allowFocusBeam = true;
		}

		var flag = !_data.lastKnownSensor.isPlayerVisible && _data.lastKnownSensor.isIlluminatedByPlayer && _numTimesIlluminatedByPlayer > 2;
		if (_data.isPlayerLocationKnown)
		{
			_sawPlayerOccluded = false;
			_movingToSearchLocation = false;
			_arrivedAtTargetSearchPosition = false;
			_searchNodesNearTarget = false;
			_searchNodesComplete = false;
		}
		else if (!_movingToSearchLocation && (_data.lostPlayerDueToOcclusion || flag))
		{
			_movingToSearchLocation = true;
			_sawPlayerOccluded = _data.lostPlayerDueToOcclusion;
			_controller.ChangeLanternFocus(0f, 2f);
			_controller.SetLanternConcealed(true, true);
			if (_allowFocusBeam)
			{
				_searchNodesNearTarget = true;
				_searchStartPosition = _controller.GetLocalFeetPosition();
				_searchNode = _controller.GetNodeMap().FindClosestNode(_data.lastKnownPlayerLocation.localPosition);
				_controller.PathfindToLocalPosition(_searchNode.localPosition, MoveType.INVESTIGATE);
			}
			else
			{
				_searchNodesNearTarget = false;
				_controller.PathfindToLocalPosition(_data.lastKnownPlayerLocation.localPosition, MoveType.INVESTIGATE);
			}

			_controller.FaceVelocity();
		}

		if (_movingToSearchLocation)
		{
			if (_arrivedAtTargetSearchPosition)
			{
				if (_searchNodesNearTarget)
				{
					_checkingTargetLocation = _searchNodesComplete;
					return;
				}

				if (_controller.GetAngleToLocalPosition(_searchPosition) < 5f)
				{
					_controller.SetLanternConcealed(false, true);
					_checkingTargetLocation = true;
					return;
				}
			}
		}
		else
		{
			var localPos = _data.lastKnownPlayerLocation.localPosition + new Vector3(0f, 1.8f, 0f);
			var flag2 = _sensors.AttachedObject.CheckPositionOccluded(_controller.LocalToWorldPosition(localPos));
			var num = _allowFocusBeam ? (_controller.GetFocusedLanternRange() - 3f) : (_controller.GetUnfocusedLanternRange() - 1f);
			var flag3 = _data.lastKnownPlayerLocation.distance < _controller.GetUnfocusedLanternRange();
			if (_data.sensor.isPlayerIlluminatedByUs)
			{
				_allowFocusBeam = true;
				_controller.FaceLocalPosition(_data.lastKnownPlayerLocation.localPosition, TurnSpeed.MEDIUM);
				if (flag3 == _controller.IsLanternFocused())
				{
					_controller.ChangeLanternFocus(flag3 ? 0f : 1f, 2f);
					return;
				}
			}
			else if (_data.lastKnownPlayerLocation.distance < num && !flag2)
			{
				if (_allowFocusBeam || !_data.isPlayerLocationKnown)
				{
					_controller.StopMoving();
				}

				if (_data.lastKnownPlayerLocation.degreesToPositionXZ < 5f && (flag3 || _controller.IsLanternFocused()))
				{
					_checkingTargetLocation = true;
				}

				if (flag3)
				{
					_controller.FaceLocalPosition(_data.lastKnownPlayerLocation.localPosition, TurnSpeed.FASTEST);
					_controller.SetLanternConcealed(false, true);
					_controller.ChangeLanternFocus(0f, 2f);
					return;
				}

				_controller.FaceLocalPosition(_data.lastKnownPlayerLocation.localPosition, TurnSpeed.MEDIUM);
				if (_data.lastKnownPlayerLocation.degreesToPositionXZ < 5f)
				{
					_controller.ChangeLanternFocus(1f, 2f);
					return;
				}
			}
			else
			{
				_controller.ChangeLanternFocus(0f, 2f);
				_controller.SetLanternConcealed(true, true);
				_controller.PathfindToLocalPosition(_data.lastKnownPlayerLocation.localPosition, MoveType.INVESTIGATE);
				_controller.FaceLocalPosition(_data.lastKnownPlayerLocation.localPosition, TurnSpeed.MEDIUM);
			}
		}
	}

	public override void OnArriveAtPosition()
	{
		if (_movingToSearchLocation)
		{
			_arrivedAtTargetSearchPosition = true;
			if (_searchNodesNearTarget)
			{
				var num = GenerateSpotlightList(_searchNode, _searchStartPosition - _searchNode.localPosition);
				if (num > 0)
				{
					_controller.SetLanternConcealed(false, true);
					_controller.ChangeLanternFocus(1f, 2f);
					_controller.FaceNodeList(IdentifyIntruderAction.s_nodesToSpotlight, num, TurnSpeed.MEDIUM, 1f, false);
					return;
				}
			}
			else
			{
				if (_sawPlayerOccluded)
				{
					_searchPosition = _data.lastKnownPlayerLocation.localPosition + _data.lastKnownPlayerLocation.localVelocity;
					_controller.FaceLocalPosition(_searchPosition, TurnSpeed.MEDIUM);
					return;
				}
			}
		}
	}

	public override void OnFinishFaceNodeList()
	{
		_searchNodesComplete = true;
	}

	private int GenerateSpotlightList(GhostNode node, Vector3 ignoreDirection)
	{
		var num = 0;
		var localPosition = node.localPosition;
		for (var i = 0; i < node.neighbors.Count; i++)
		{
			if (Vector3.Angle(node.neighbors[i].localPosition - localPosition, ignoreDirection) >= 45f)
			{
				IdentifyIntruderAction.s_nodesToSpotlight[num] = node.neighbors[i];
				num++;
				if (num == IdentifyIntruderAction.s_nodesToSpotlight.Length)
				{
					break;
				}
			}
		}
		return num;
	}
}
