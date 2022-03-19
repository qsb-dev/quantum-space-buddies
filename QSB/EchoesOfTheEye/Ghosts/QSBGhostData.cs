using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts;

public class QSBGhostData
{
	public GhostLocationData playerLocation = new GhostLocationData();
	public GhostLocationData lastKnownPlayerLocation = new GhostLocationData();
	public GhostSensorData sensor = new GhostSensorData();
	public GhostSensorData lastKnownSensor = new GhostSensorData();
	private GhostSensorData firstUnknownSensor = new GhostSensorData();
	public GhostData.ThreatAwareness threatAwareness;
	public GhostAction.Name currentAction = GhostAction.Name.None;
	public GhostAction.Name previousAction = GhostAction.Name.None;
	public bool isAlive = true;
	public bool hasWokenUp;
	public bool isPlayerLocationKnown;
	public bool wasPlayerLocationKnown;
	public bool reduceGuardUtility;
	public bool fastStalkUnlocked;
	public float timeLastSawPlayer;
	public float timeSincePlayerLocationKnown = float.PositiveInfinity;
	public float playerMinLanternRange;
	public float illuminatedByPlayerMeter;
	public bool hasChokePoint;
	public Vector3 chokePointLocalPosition;
	public Vector3 chokePointLocalFacing;
	public bool reducedFrights_allowChase;
	public bool lostPlayerDueToOcclusion => !isPlayerLocationKnown && !lastKnownSensor.isPlayerOccluded && firstUnknownSensor.isPlayerOccluded;

	public void TabulaRasa()
	{
		threatAwareness = GhostData.ThreatAwareness.EverythingIsNormal;
		isPlayerLocationKnown = false;
		wasPlayerLocationKnown = false;
		reduceGuardUtility = false;
		fastStalkUnlocked = false;
		timeLastSawPlayer = 0f;
		timeSincePlayerLocationKnown = float.PositiveInfinity;
		playerMinLanternRange = 0f;
		illuminatedByPlayerMeter = 0f;
	}

	public void OnPlayerExitDreamWorld()
	{
		isPlayerLocationKnown = false;
		wasPlayerLocationKnown = false;
		reduceGuardUtility = false;
		fastStalkUnlocked = false;
		timeSincePlayerLocationKnown = float.PositiveInfinity;
	}

	public void OnEnterAction(GhostAction.Name actionName)
	{
		if (actionName == GhostAction.Name.IdentifyIntruder || actionName - GhostAction.Name.Chase <= 2)
		{
			reduceGuardUtility = true;
		}
	}

	public void FixedUpdate_Data(GhostController controller, GhostSensors sensors)
	{
		wasPlayerLocationKnown = isPlayerLocationKnown;
		isPlayerLocationKnown = sensor.isPlayerVisible || sensor.isPlayerHeldLanternVisible || sensor.isIlluminatedByPlayer || sensor.inContactWithPlayer;
		if (!reduceGuardUtility && sensor.isIlluminatedByPlayer)
		{
			reduceGuardUtility = true;
		}

		var worldPosition = Locator.GetPlayerTransform().position - Locator.GetPlayerTransform().up;
		var worldVelocity = Locator.GetPlayerBody().GetVelocity() - controller.GetNodeMap().GetOWRigidbody().GetVelocity();
		playerLocation.Update(worldPosition, worldVelocity, controller);
		playerMinLanternRange = Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController().GetMinRange();
		if (isPlayerLocationKnown)
		{
			lastKnownPlayerLocation.CopyFromOther(playerLocation);
			lastKnownSensor.CopyFromOther(sensor);
			timeLastSawPlayer = Time.time;
			timeSincePlayerLocationKnown = 0f;
		}
		else
		{
			if (wasPlayerLocationKnown)
			{
				firstUnknownSensor.CopyFromOther(sensor);
			}

			lastKnownPlayerLocation.Update(controller);
			timeSincePlayerLocationKnown += Time.deltaTime;
		}

		if (threatAwareness >= GhostData.ThreatAwareness.IntruderConfirmed && sensor.isIlluminatedByPlayer && !PlayerData.GetReducedFrights())
		{
			illuminatedByPlayerMeter += Time.deltaTime;
			return;
		}

		illuminatedByPlayerMeter = Mathf.Max(0f, illuminatedByPlayerMeter - (Time.deltaTime * 0.5f));
	}
}
