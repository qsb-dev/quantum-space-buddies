using HarmonyLib;
using QSB.EchoesOfTheEye.Ghosts.Actions;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Patches;
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

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GhostZone2Director), nameof(GhostZone2Director.Update))]
	public static bool Update(GhostZone2Director __instance)
	{
		if (__instance._lightsProjectorExtinguished)
		{
			if (__instance._ghostsAreAwake && !__instance._ghostsAlerted && Time.time >= __instance._ghostAlertTime)
			{
				__instance._ghostHowlAudioSource.PlayOneShot(global::AudioType.Ghost_SomeoneIsInHereHowl, 1f);
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
				}

				if (QSBGhostZone2Director.ElevatorsStatus[i].lightsDeactivated && QSBGhostZone2Director.ElevatorsStatus[i].activated && !QSBGhostZone2Director.ElevatorsStatus[i].deactivated && Time.time >= QSBGhostZone2Director.ElevatorsStatus[i].timeSinceArrival + 2f)
				{
					QSBGhostZone2Director.ElevatorsStatus[i].elevatorPair.elevator.GoToDestination(1);
					QSBGhostZone2Director.ElevatorsStatus[i].deactivated = true;
				}
			}
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GhostZone2Director), nameof(GhostZone2Director.OnLightsExtinguished))]
	public static bool OnLightsExtinguished(GhostZone2Director __instance)
	{
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
			QSBGhostZone2Director.ElevatorsStatus[j].elevatorAction = __instance._elevators[j].ghost.GetWorldObject<QSBGhostBrain>().GetAction(GhostAction.Name.ElevatorWalk) as QSBElevatorWalkAction;
			DebugLog.DebugWrite($"- CallToUseElevator on action");
			QSBGhostZone2Director.ElevatorsStatus[j].elevatorAction.CallToUseElevator();
			DebugLog.DebugWrite($"- get ghost controller");
			QSBGhostZone2Director.ElevatorsStatus[j].ghostController = QSBGhostZone2Director.ElevatorsStatus[j].elevatorPair.ghost.GetComponent<GhostController>();
		}

		__instance._ghostAlertTime = Time.time + 2f;

		return false;
	}
}
