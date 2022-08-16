using QSB.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts;

public class QSBGhostData
{
	public GhostData.ThreatAwareness threatAwareness;
	public GhostAction.Name currentAction = GhostAction.Name.None;
	public GhostAction.Name previousAction = GhostAction.Name.None;
	public bool isAlive = true;
	public bool hasWokenUp;
	public bool reduceGuardUtility;
	public bool fastStalkUnlocked;
	public float illuminatedByPlayerMeter;
	public bool reducedFrights_allowChase;
	public bool isIlluminated;
	public bool IsIlluminatedByAnyPlayer => players.Values.Any(x => x.sensor.isIlluminatedByPlayer);
	public Dictionary<PlayerInfo, GhostPlayer> players = new();
	public GhostPlayer localPlayer => players[QSBPlayerManager.LocalPlayer];
	public GhostPlayer interestedPlayer;

	public void TabulaRasa()
	{
		threatAwareness = GhostData.ThreatAwareness.EverythingIsNormal;
		reduceGuardUtility = false;
		fastStalkUnlocked = false;
		illuminatedByPlayerMeter = 0f;

		foreach (var player in players.Values)
		{
			player.isPlayerLocationKnown = false;
			player.wasPlayerLocationKnown = false;
			player.timeLastSawPlayer = 0f;
			player.timeSincePlayerLocationKnown = float.PositiveInfinity;
			player.playerMinLanternRange = 0f;
		}
	}

	public void OnPlayerExitDreamWorld(PlayerInfo player)
	{
		players[player].isPlayerLocationKnown = false;
		players[player].wasPlayerLocationKnown = false;
		players[player].timeSincePlayerLocationKnown = float.PositiveInfinity;
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
		foreach (var player in QSBPlayerManager.PlayerList)
		{
			if (!players.ContainsKey(player))
			{
				var newPlayer = new GhostPlayer
				{
					player = player
				};
				players.Add(player, newPlayer);
			}
		}

		foreach (var pair in players)
		{
			var player = pair.Value;

			if (!player.player.InDreamWorld)
			{
				continue;
			}

			player.wasPlayerLocationKnown = player.isPlayerLocationKnown;
			player.isPlayerLocationKnown = player.sensor.isPlayerVisible
				|| player.sensor.isPlayerHeldLanternVisible
				|| player.sensor.isIlluminatedByPlayer
				|| player.sensor.inContactWithPlayer;
			if (!reduceGuardUtility && player.sensor.isIlluminatedByPlayer)
			{
				reduceGuardUtility = true;
			}

			var worldPosition = pair.Key.Body.transform.position - pair.Key.Body.transform.up;
			var worldVelocity = pair.Key.Velocity - controller.GetNodeMap().GetOWRigidbody().GetVelocity();
			player.playerLocation.Update(worldPosition, worldVelocity, controller);
			player.playerMinLanternRange = pair.Key.AssignedSimulationLantern.AttachedObject.GetLanternController().GetMinRange();
			if (player.isPlayerLocationKnown)
			{
				player.lastKnownPlayerLocation.CopyFromOther(player.playerLocation);
				player.lastKnownSensor.CopyFromOther(player.sensor);
				player.timeLastSawPlayer = Time.time;
				player.timeSincePlayerLocationKnown = 0f;
			}
			else
			{
				if (player.wasPlayerLocationKnown)
				{
					player.firstUnknownSensor.CopyFromOther(player.sensor);
				}

				player.lastKnownPlayerLocation.Update(controller);
				player.timeSincePlayerLocationKnown += Time.deltaTime;
			}
		}

		if (threatAwareness >= GhostData.ThreatAwareness.IntruderConfirmed && IsIlluminatedByAnyPlayer && !PlayerData.GetReducedFrights())
		{
			illuminatedByPlayerMeter += Time.deltaTime;
			return;
		}

		illuminatedByPlayerMeter = Mathf.Max(0f, illuminatedByPlayerMeter - (Time.deltaTime * 0.5f));
	}
}
