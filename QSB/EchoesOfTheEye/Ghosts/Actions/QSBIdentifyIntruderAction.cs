using System;
using GhostEnums;
using QSB;
using QSB.EchoesOfTheEye.Ghosts;
using QSB.EchoesOfTheEye.Ghosts.Messages;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
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
		if (_data.interestedPlayer == null)
		{
			return -100f;
		}

		if (_data.threatAwareness >= GhostData.ThreatAwareness.IntruderConfirmed)
		{
			return -100f;
		}

		if (_running
			|| (_data.interestedPlayer.sensor.isPlayerHeldLanternVisible
				&& (_data.threatAwareness > GhostData.ThreatAwareness.EverythingIsNormal || _data.interestedPlayer.playerLocation.distance < 20f)
				&& _controller.AttachedObject.GetNodeMap().CheckLocalPointInBounds(_data.interestedPlayer.playerLocation.localPosition))
			|| _data.interestedPlayer.sensor.isIlluminatedByPlayer)
		{
			return 80f;
		}

		return -100f;
	}

	public override float GetActionDelay()
	{
		if (_data.interestedPlayer.playerLocation.distance < 8f)
		{
			return 0.1f;
		}
		return 0.5f;
	}

	public override void OnSetAsPending()
	{
		if (_data.interestedPlayer.sensor.isIlluminatedByPlayer)
		{
			_numTimesIlluminatedByPlayer++;
			_allowFocusBeam = true;
		}
	}

	protected override void OnEnterAction()
	{
		_sawPlayerOccluded = false;
		_movingToSearchLocation = false;
		_arrivedAtTargetSearchPosition = false;
		_searchNodesNearTarget = false;
		_searchNodesComplete = false;
		_checkingTargetLocation = false;
		_checkTimer = 0f;
		_effects.PlayVoiceAudioNear((_numTimesIlluminatedByPlayer <= 2) ? AudioType.Ghost_Identify_Curious : AudioType.Ghost_Identify_Irritated, 1f);
	}

	protected override void OnExitAction()
	{
		_controller.FaceVelocity();
		_allowFocusBeam = false;
	}

	public override bool Update_Action()
	{
		if (_checkingTargetLocation && !_data.interestedPlayer.isPlayerLocationKnown && _controller.AttachedObject.GetSpeed() < 0.1f)
		{
			_checkTimer += Time.deltaTime;
		}
		else
		{
			_checkTimer = 0f;
		}

		if ((_searchNodesNearTarget && _checkTimer > 1f) || _checkTimer > 3f)
		{
			DebugLog.DebugWrite($"{_brain.AttachedObject._name} : Couldn't identify target. :(");
			_effects.PlayVoiceAudioNear(AudioType.Ghost_Identify_Fail, 1f);
			return false;
		}

		return true;
	}

	public override void FixedUpdate_Action()
	{
		_checkingTargetLocation = false;
		if (!_data.interestedPlayer.wasPlayerLocationKnown && _data.interestedPlayer.isPlayerLocationKnown && _data.interestedPlayer.sensor.isIlluminatedByPlayer)
		{
			_numTimesIlluminatedByPlayer++;
		}

		if (!_allowFocusBeam && _data.interestedPlayer.sensor.isIlluminatedByPlayer)
		{
			_allowFocusBeam = true;
		}

		var flag = !_data.interestedPlayer.lastKnownSensor.isPlayerVisible && _data.interestedPlayer.lastKnownSensor.isIlluminatedByPlayer && _numTimesIlluminatedByPlayer > 2;
		if (_data.interestedPlayer.isPlayerLocationKnown)
		{
			_sawPlayerOccluded = false;
			_movingToSearchLocation = false;
			_arrivedAtTargetSearchPosition = false;
			_searchNodesNearTarget = false;
			_searchNodesComplete = false;
		}
		else if (!_movingToSearchLocation && (_data.interestedPlayer.lostPlayerDueToOcclusion || flag))
		{
			_movingToSearchLocation = true;
			_sawPlayerOccluded = _data.interestedPlayer.lostPlayerDueToOcclusion;
			_controller.ChangeLanternFocus(0f, 2f);
			_controller.SetLanternConcealed(true, true);
			if (_allowFocusBeam)
			{
				_searchNodesNearTarget = true;
				_searchStartPosition = _controller.AttachedObject.GetLocalFeetPosition();
				_searchNode = _controller.AttachedObject.GetNodeMap().FindClosestNode(_data.interestedPlayer.lastKnownPlayerLocation.localPosition);
				_controller.PathfindToLocalPosition(_searchNode.localPosition, MoveType.INVESTIGATE);
			}
			else
			{
				_searchNodesNearTarget = false;
				_controller.PathfindToLocalPosition(_data.interestedPlayer.lastKnownPlayerLocation.localPosition, MoveType.INVESTIGATE);
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

				if (_controller.AttachedObject.GetAngleToLocalPosition(_searchPosition) < 5f)
				{
					_controller.SetLanternConcealed(false, true);
					_checkingTargetLocation = true;
					return;
				}
			}
		}
		else
		{
			var playerLocationToCheck = _data.interestedPlayer.lastKnownPlayerLocation.localPosition + new Vector3(0f, 1.8f, 0f);
			var canSeePlayerCheckLocation = _sensors.AttachedObject.CheckPositionOccluded(_controller.AttachedObject.LocalToWorldPosition(playerLocationToCheck));
			var lanternRange = _allowFocusBeam
				? (_controller.AttachedObject.GetFocusedLanternRange() - 3f)
				: (_controller.AttachedObject.GetUnfocusedLanternRange() - 1f);
			var isLastKnownLocationInRange = _data.interestedPlayer.lastKnownPlayerLocation.distance < _controller.AttachedObject.GetUnfocusedLanternRange();
			if (_data.interestedPlayer.sensor.isPlayerIlluminatedByUs)
			{
				_allowFocusBeam = true;
				_controller.FaceLocalPosition(_data.interestedPlayer.lastKnownPlayerLocation.localPosition, TurnSpeed.MEDIUM);
				if (isLastKnownLocationInRange == _controller.AttachedObject.IsLanternFocused())
				{
					_controller.ChangeLanternFocus(isLastKnownLocationInRange ? 0f : 1f, 2f);
					return;
				}
			}
			else if (_data.interestedPlayer.lastKnownPlayerLocation.distance < lanternRange && !canSeePlayerCheckLocation)
			{
				if (_allowFocusBeam || !_data.interestedPlayer.isPlayerLocationKnown)
				{
					_controller.StopMoving();
				}

				if (_data.interestedPlayer.lastKnownPlayerLocation.degreesToPositionXZ < 5f && (isLastKnownLocationInRange || _controller.AttachedObject.IsLanternFocused()))
				{
					_checkingTargetLocation = true;
				}

				if (isLastKnownLocationInRange)
				{
					_controller.FaceLocalPosition(_data.interestedPlayer.lastKnownPlayerLocation.localPosition, TurnSpeed.FASTEST);
					_controller.SetLanternConcealed(false, true);
					_controller.ChangeLanternFocus(0f, 2f);
					return;
				}

				_controller.FaceLocalPosition(_data.interestedPlayer.lastKnownPlayerLocation.localPosition, TurnSpeed.MEDIUM);
				if (_data.interestedPlayer.lastKnownPlayerLocation.degreesToPositionXZ < 5f)
				{
					_controller.ChangeLanternFocus(1f, 2f);
					return;
				}
			}
			else
			{
				_controller.ChangeLanternFocus(0f, 2f);
				_controller.SetLanternConcealed(true, true);

				if (!QSBCore.IsHost)
				{
					return;
				}

				_controller.PathfindToLocalPosition(_data.interestedPlayer.lastKnownPlayerLocation.localPosition, MoveType.INVESTIGATE);
				_controller.FaceLocalPosition(_data.interestedPlayer.lastKnownPlayerLocation.localPosition, TurnSpeed.MEDIUM);
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
					_searchPosition = _data.interestedPlayer.lastKnownPlayerLocation.localPosition + _data.interestedPlayer.lastKnownPlayerLocation.localVelocity;
					_controller.FaceLocalPosition(_searchPosition, TurnSpeed.MEDIUM);
					return;
				}

				DebugLog.DebugWrite($"{_brain.Name} : how did i get here\nthis is not my beautiful house\nthis is not my beautiful wife", OWML.Common.MessageType.Error);
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
