using HarmonyLib;
using QSB.EchoesOfTheEye.Ghosts.Actions;
using QSB.EchoesOfTheEye.Ghosts.Messages;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Patches;

internal class GhostZone2DirectorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(GhostDirector), nameof(GhostDirector.Awake))]
	public static void GhostDirector_Awake_Stub(object instance) { }

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GhostZone2Director), nameof(GhostZone2Director.Awake))]
	public static bool Awake(GhostZone2Director __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		GhostDirector_Awake_Stub(__instance);

		QSBGhostZone2Director.ElevatorsStatus = new QSBGhostZone2Director.ElevatorStatus[__instance._elevators.Length];
		for (var j = 0; j < __instance._elevators.Length; j++)
		{
			QSBGhostZone2Director.ElevatorsStatus[j].elevatorPair = __instance._elevators[j];
			QSBGhostZone2Director.ElevatorsStatus[j].activated = false;
			QSBGhostZone2Director.ElevatorsStatus[j].deactivated = false;
			QSBGhostZone2Director.ElevatorsStatus[j].lightsDeactivated = false;
		}

		return false;
	}

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(GhostDirector), nameof(GhostDirector.OnDestroy))]
	public static void GhostDirector_OnDestroy_Stub(object instance) { }

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GhostZone2Director), nameof(GhostZone2Director.OnDestroy))]
	public static bool OnDestroy(GhostZone2Director __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		GhostDirector_OnDestroy_Stub(__instance);

		__instance._lightsProjector.OnProjectorExtinguished -= __instance.OnLightsExtinguished;
		__instance._undergroundVolume.OnEntry -= __instance.OnEnterUnderground;
		__instance._undergroundVolume.OnExit -= __instance.OnExitUnderground;
		__instance._finalTotem.OnRinging -= __instance.OnAlarmRinging;
		for (var i = 0; i < __instance._cityGhosts.Length; i++)
		{
			__instance._cityGhosts[i].GetWorldObject<QSBGhostBrain>().OnIdentifyIntruder -= GhostManager.CustomOnCityGhostsIdentifiedIntruder;
		}

		__instance._ghostTutorialArrival.OnEntry -= __instance.OnStartGhostTutorial;

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GhostZone2Director), nameof(GhostZone2Director.Update))]
	public static bool Update(GhostZone2Director __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		if (!QSBCore.IsHost)
		{
			return false;
		}

		if (__instance._lightsProjectorExtinguished)
		{
			if (__instance._ghostsAreAwake && !__instance._ghostsAlerted && Time.time >= __instance._ghostAlertTime)
			{
				__instance._ghostHowlAudioSource.PlayOneShot(AudioType.Ghost_SomeoneIsInHereHowl, 1f);
				__instance._ghostsAlerted = true;
			}

			for (var i = 0; i < QSBGhostZone2Director.ElevatorsStatus.Length; i++)
			{
				if (!QSBGhostZone2Director.ElevatorsStatus[i].activated && QSBGhostZone2Director.ElevatorsStatus[i].elevatorAction.reachedEndOfPath)
				{
					QSBGhostZone2Director.ElevatorsStatus[i].ghostController.SetNodeMap(QSBGhostZone2Director.ElevatorsStatus[i].elevatorPair.nodeMap);
					QSBGhostZone2Director.ElevatorsStatus[i].elevatorPair.elevator.topLight.FadeTo(1f, 0.2f);
					QSBGhostZone2Director.ElevatorsStatus[i].elevatorPair.elevator.GoToDestination(0);
					QSBGhostZone2Director.ElevatorsStatus[i].activated = true;
					new Zone2ElevatorStateMessage(i, Zone2ElevatorState.GoToUndercity).Send();
				}

				if (!QSBGhostZone2Director.ElevatorsStatus[i].lightsDeactivated && QSBGhostZone2Director.ElevatorsStatus[i].activated && QSBGhostZone2Director.ElevatorsStatus[i].elevatorPair.elevator.isAtBottom)
				{
					QSBGhostZone2Director.ElevatorsStatus[i].lightsDeactivated = true;
					QSBGhostZone2Director.ElevatorsStatus[i].elevatorPair.elevator.topLight.FadeTo(0f, 0.2f);
					if (QSBGhostZone2Director.ElevatorsStatus[i].elevatorPair.cityDestination)
					{
						QSBGhostZone2Director.ElevatorsStatus[i].ghostController.SetNodeMap(__instance._cityNodeMap);
					}
					else
					{
						QSBGhostZone2Director.ElevatorsStatus[i].ghostController.SetNodeMap(__instance._undercityNodeMap);
					}

					if (i == 1)
					{
						QSBGhostZone2Director.ElevatorsStatus[i].ghostController.gameObject.GetComponent<Transform>().position = __instance._teleportNode.position;
					}

					QSBGhostZone2Director.ElevatorsStatus[i].elevatorAction.UseElevator();
					QSBGhostZone2Director.ElevatorsStatus[i].timeSinceArrival = Time.time;
					new Zone2ElevatorStateMessage(i, Zone2ElevatorState.ReachedUndercity).Send();
				}

				if (QSBGhostZone2Director.ElevatorsStatus[i].lightsDeactivated && QSBGhostZone2Director.ElevatorsStatus[i].activated && !QSBGhostZone2Director.ElevatorsStatus[i].deactivated && Time.time >= QSBGhostZone2Director.ElevatorsStatus[i].timeSinceArrival + 2f)
				{
					QSBGhostZone2Director.ElevatorsStatus[i].elevatorPair.elevator.GoToDestination(1);
					QSBGhostZone2Director.ElevatorsStatus[i].deactivated = true;
					new Zone2ElevatorStateMessage(i, Zone2ElevatorState.ReturnToCity).Send();
				}
			}
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GhostZone2Director), nameof(GhostZone2Director.OnStartGhostTutorial))]
	public static bool OnStartGhostTutorial(GhostZone2Director __instance, GameObject hitObj)
	{
		if (__instance._lightsProjectorExtinguished && hitObj.CompareTag("PlayerDetector") && !__instance._ghostTutorialElevator.isAtTop)
		{
			__instance._ghostTutorialElevator.GoToDestination(1);
			new Zone2ElevatorStateMessage(-1, Zone2ElevatorState.TutorialElevator).Send();
			for (var i = 0; i < __instance._cityGhosts.Length; i++)
			{
				__instance._cityGhosts[i].EscalateThreatAwareness(GhostData.ThreatAwareness.IntruderConfirmed);
			}
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GhostZone2Director), nameof(GhostZone2Director.OnLightsExtinguished))]
	public static bool OnLightsExtinguished(GhostZone2Director __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		if (!QSBCore.IsHost)
		{
			return false;
		}

		DebugLog.DebugWrite($"LIGHTS EXTINGUISHED");
		__instance._lightsProjectorExtinguished = true;
		__instance.WakeGhosts();

		if (QSBGhostZone2Director.ElevatorsStatus == null)
		{
			QSBGhostZone2Director.ElevatorsStatus = new QSBGhostZone2Director.ElevatorStatus[__instance._elevators.Length];
			for (var j = 0; j < __instance._elevators.Length; j++)
			{
				QSBGhostZone2Director.ElevatorsStatus[j].elevatorPair = __instance._elevators[j];
				QSBGhostZone2Director.ElevatorsStatus[j].activated = false;
				QSBGhostZone2Director.ElevatorsStatus[j].deactivated = false;
				QSBGhostZone2Director.ElevatorsStatus[j].lightsDeactivated = false;
			}
		}

		DebugLog.DebugWrite($"ESCALATE THREAT AWARENESS");
		for (var i = 0; i < __instance._directedGhosts.Length; i++)
		{
			__instance._directedGhosts[i].EscalateThreatAwareness(GhostData.ThreatAwareness.SomeoneIsInHere);
			__instance._directedGhosts[i].GetWorldObject<QSBGhostBrain>().GetEffects().CancelStompyFootsteps();
		}

		DebugLog.DebugWrite($"SETUP ELEVATOR STATUS");
		for (var j = 0; j < QSBGhostZone2Director.ElevatorsStatus.Length; j++)
		{
			DebugLog.DebugWrite($"[{j}]");
			DebugLog.DebugWrite($"- fade light down");
			QSBGhostZone2Director.ElevatorsStatus[j].elevatorPair.elevator.topLight.FadeTo(0f, 0.2f);
			DebugLog.DebugWrite($"- get action");
			// BUG: breaks on client cuz cast
			QSBGhostZone2Director.ElevatorsStatus[j].elevatorAction = (QSBElevatorWalkAction)__instance._elevators[j].ghost.GetWorldObject<QSBGhostBrain>().GetAction(GhostAction.Name.ElevatorWalk);
			DebugLog.DebugWrite($"- CallToUseElevator on action");
			QSBGhostZone2Director.ElevatorsStatus[j].elevatorAction.CallToUseElevator();
			DebugLog.DebugWrite($"- get ghost controller");
			QSBGhostZone2Director.ElevatorsStatus[j].ghostController = QSBGhostZone2Director.ElevatorsStatus[j].elevatorPair.ghost.GetComponent<GhostController>();
		}

		new Zone2ElevatorStateMessage(-1, Zone2ElevatorState.LightsExtinguished).Send();

		__instance._ghostAlertTime = Time.time + 2f;

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GhostZone2Director), nameof(GhostZone2Director.OnAlarmRinging))]
	public static bool OnAlarmRinging(GhostZone2Director __instance)
	{
		foreach (var ghost in __instance._undergroundGhosts)
		{
			var qsbGhost = ghost.GetWorldObject<QSBGhostBrain>();
			var totemPos = __instance._finalTotem.transform.position;
			var closestPlayer = QSBPlayerManager.GetClosestPlayerToWorldPoint(totemPos, true);
			qsbGhost.HintPlayerLocation(totemPos, 0, qsbGhost._data.players[closestPlayer]);
		}

		return false;
	}
}
