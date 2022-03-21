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
	public override void SendInitialState(uint to)
	{

	}

	public override bool ShouldDisplayDebug() => false;

	private QSBGhostData _data;

	public void Initialize(QSBGhostData data, OWTriggerVolume guardVolume = null)
	{
		_data = data;
		AttachedObject._origEdgeCatcherSize = AttachedObject._contactEdgeCatcherShape.size;
		AttachedObject._guardVolume = guardVolume;
	}

	public bool CanGrabPlayer()
		=> !PlayerState.IsAttached()
			&& _data.playerLocation.distanceXZ < 2f + AttachedObject._grabDistanceBuff
			&& _data.playerLocation.degreesToPositionXZ < 20f + AttachedObject._grabAngleBuff
			&& AttachedObject._animator.GetFloat("GrabWindow") > 0.5f;

	public void FixedUpdate_Sensors()
	{
		if (_data == null)
		{
			return;
		}

		var lanternController = Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController();
		var playerLightSensor = Locator.GetPlayerLightSensor();
		_data.sensor.isPlayerHoldingLantern = lanternController.IsHeldByPlayer();
		_data.sensor.isIlluminated = AttachedObject._lightSensor.IsIlluminated();
		_data.sensor.isIlluminatedByPlayer = (lanternController.IsHeldByPlayer() && AttachedObject._lightSensor.IsIlluminatedByLantern(lanternController));
		_data.sensor.isPlayerIlluminatedByUs = playerLightSensor.IsIlluminatedByLantern(AttachedObject._lantern);
		_data.sensor.isPlayerIlluminated = playerLightSensor.IsIlluminated();
		_data.sensor.isPlayerVisible = false;
		_data.sensor.isPlayerHeldLanternVisible = false;
		_data.sensor.isPlayerDroppedLanternVisible = false;
		_data.sensor.isPlayerOccluded = false;

		if ((lanternController.IsHeldByPlayer() && !lanternController.IsConcealed()) || playerLightSensor.IsIlluminated())
		{
			var position = Locator.GetPlayerCamera().transform.position;
			if (AttachedObject.CheckPointInVisionCone(position))
			{
				if (AttachedObject.CheckLineOccluded(AttachedObject._sightOrigin.position, position))
				{
					_data.sensor.isPlayerOccluded = true;
				}
				else
				{
					_data.sensor.isPlayerVisible = playerLightSensor.IsIlluminated();
					_data.sensor.isPlayerHeldLanternVisible = (lanternController.IsHeldByPlayer() && !lanternController.IsConcealed());
				}
			}
		}

		if (!lanternController.IsHeldByPlayer() && AttachedObject.CheckPointInVisionCone(lanternController.GetLightPosition()) && !AttachedObject.CheckLineOccluded(AttachedObject._sightOrigin.position, lanternController.GetLightPosition()))
		{
			_data.sensor.isPlayerDroppedLanternVisible = true;
		}
	}

	public void OnEnterContactTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_data.sensor.inContactWithPlayer = true;
		}
	}

	public void OnExitContactTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_data.sensor.inContactWithPlayer = false;
		}
	}
}