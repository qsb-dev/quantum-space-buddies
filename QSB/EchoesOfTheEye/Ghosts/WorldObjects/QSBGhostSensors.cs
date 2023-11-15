using QSB.EchoesOfTheEye.Ghosts.Messages;
using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.WorldObjects;

public class QSBGhostSensors : WorldObject<GhostSensors>, IGhostObject
{
	public override string ReturnLabel() => "";

	public override void DisplayLines()
	{
		var rotation = Quaternion.AngleAxis(20f, AttachedObject.transform.up);
		var b = rotation * (AttachedObject.transform.forward * 50f);
		var b2 = Quaternion.Inverse(rotation) * (AttachedObject.transform.forward * 50f);
		var color = Color.blue;
		Popcron.Gizmos.Line(AttachedObject._sightOrigin.position, AttachedObject._sightOrigin.position + b, color);
		Popcron.Gizmos.Line(AttachedObject._sightOrigin.position, AttachedObject._sightOrigin.position + b2, color);
		Popcron.Gizmos.Line(AttachedObject._sightOrigin.position + AttachedObject.transform.right * 0.5f, AttachedObject._sightOrigin.position + AttachedObject.transform.forward + AttachedObject.transform.right * 0.5f, color);
		Popcron.Gizmos.Line(AttachedObject._sightOrigin.position - AttachedObject.transform.right * 0.5f, AttachedObject._sightOrigin.position + AttachedObject.transform.forward - AttachedObject.transform.right * 0.5f, color);
		Popcron.Gizmos.Line(AttachedObject._sightOrigin.position + AttachedObject.transform.forward + AttachedObject.transform.right * 0.5f, AttachedObject._sightOrigin.position + AttachedObject.transform.forward - AttachedObject.transform.right * 0.5f, color);
	}

	public QSBGhostData _data;

	public void Initialize(QSBGhostData data)
	{
		_data = data;
		AttachedObject._contactTrigger.OnEntry -= AttachedObject.OnEnterContactTrigger;
		AttachedObject._contactTrigger.OnEntry += OnEnterContactTrigger;
		AttachedObject._contactTrigger.OnExit -= AttachedObject.OnExitContactTrigger;
		AttachedObject._contactTrigger.OnExit += OnExitContactTrigger;
		AttachedObject._origEdgeCatcherSize = AttachedObject._contactEdgeCatcherShape.size;
	}

	public bool CanGrabPlayer(GhostPlayer player)
		=> !PlayerState.IsAttached() // TODO : check for each player
			&& player.playerLocation.distanceXZ < 2f + AttachedObject._grabDistanceBuff
			&& player.playerLocation.toPosition.y > -2f
			&& player.playerLocation.toPosition.y < 3f
			&& player.playerLocation.degreesToPositionXZ < 20f + AttachedObject._grabAngleBuff
			&& AttachedObject._animator.GetFloat("GrabWindow") > 0.5f;

	public void FixedUpdate_Sensors()
	{
		if (_data == null)
		{
			return;
		}

		foreach (var pair in _data.players)
		{
			var player = pair.Value;

			if (player.player.AssignedSimulationLantern == null)
			{
				continue;
			}

			var lanternController = player.player.AssignedSimulationLantern.AttachedObject.GetLanternController();
			var playerLightSensor = player.player.LightSensor;
			player.sensor.isPlayerHoldingLantern = lanternController.IsHeldByPlayer();
			_data.isIlluminated = AttachedObject._lightSensor.IsIlluminated();
			player.sensor.isIlluminatedByPlayer = (lanternController.IsHeldByPlayer() && AttachedObject._lightSensor.IsIlluminatedByLantern(lanternController));
			player.sensor.isPlayerIlluminatedByUs = playerLightSensor.IsIlluminatedByLantern(AttachedObject._lantern);
			player.sensor.isPlayerIlluminated = playerLightSensor.IsIlluminated();
			player.sensor.isPlayerVisible = false;
			player.sensor.isPlayerHeldLanternVisible = false;
			player.sensor.isPlayerDroppedLanternVisible = false;
			player.sensor.isPlayerOccluded = false;

			if ((lanternController.IsHeldByPlayer() && !lanternController.IsConcealed()) || playerLightSensor.IsIlluminated())
			{
				var position = pair.Key.Camera.transform.position;
				if (AttachedObject.CheckPointInVisionCone(position))
				{
					if (AttachedObject.CheckLineOccluded(AttachedObject._sightOrigin.position, position))
					{
						player.sensor.isPlayerOccluded = true;
					}
					else
					{
						player.sensor.isPlayerVisible = playerLightSensor.IsIlluminated();
						player.sensor.isPlayerHeldLanternVisible = (lanternController.IsHeldByPlayer() && !lanternController.IsConcealed());
					}
				}
			}

			if (!lanternController.IsHeldByPlayer() && AttachedObject.CheckPointInVisionCone(lanternController.GetLightPosition()) && !AttachedObject.CheckLineOccluded(AttachedObject._sightOrigin.position, lanternController.GetLightPosition()))
			{
				player.sensor.isPlayerDroppedLanternVisible = true;
			}
		}

		if (!QSBCore.IsHost)
		{
			return;
		}

		var visiblePlayers = _data.players.Values.Where(x => x.sensor.isPlayerVisible || x.sensor.isPlayerHeldLanternVisible || x.sensor.inContactWithPlayer || x.sensor.isPlayerIlluminatedByUs);

		if (visiblePlayers.Count() == 0) // no players visible
		{
			visiblePlayers = _data.players.Values.Where(x => x.sensor.isIlluminatedByPlayer);
		}

		if (visiblePlayers.Count() == 0) // no players lighting us
		{
			return;
		}

		var closest = visiblePlayers.MinBy(x => x.playerLocation.distance);

		if (_data.interestedPlayer != closest)
		{
			_data.interestedPlayer = closest;
			this.SendMessage(new ChangeInterestedPlayerMessage(closest.player.PlayerId));
		}
	}

	public void OnEnterContactTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector") && _data.localPlayer != null && _data.localPlayer.sensor != null)
		{
			_data.localPlayer.sensor.inContactWithPlayer = true;

			if (!QSBCore.IsHost)
			{
				this.SendMessage(new ContactTriggerMessage(true));
			}
		}
	}

	public void OnExitContactTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector") && _data.localPlayer != null && _data.localPlayer.sensor != null)
		{
			_data.localPlayer.sensor.inContactWithPlayer = false;

			if (!QSBCore.IsHost)
			{
				this.SendMessage(new ContactTriggerMessage(false));
			}
		}
	}
}