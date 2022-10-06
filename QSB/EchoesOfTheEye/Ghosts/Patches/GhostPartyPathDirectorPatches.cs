using HarmonyLib;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using System.Reflection;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Patches;

[HarmonyPatch(typeof(GhostPartyPathDirector))]
internal class GhostPartyPathDirectorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(GhostDirector), nameof(GhostDirector.OnDestroy))]
	public static void GhostDirector_OnDestroy_Stub(object instance) { }

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostPartyPathDirector.OnDestroy))]
	public static bool OnDestroy(GhostPartyPathDirector __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		GhostDirector_OnDestroy_Stub(__instance);

		for (var i = 0; i < __instance._directedGhosts.Length; i++)
		{
			__instance._directedGhosts[i].GetWorldObject<QSBGhostBrain>().OnIdentifyIntruder -= GhostManager.CustomOnGhostIdentifyIntruder;
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostPartyPathDirector.Update))]
	public static bool Update(GhostPartyPathDirector __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		if (!QSBCore.IsHost)
		{
			return false;
		}

		if (__instance._connectedCampfireExtinguished)
		{
			return false;
		}

		for (var i = __instance._dispatchedGhosts.Count - 1; i >= 0; i--)
		{
			var ghostBrain = __instance._dispatchedGhosts[i].GetWorldObject<QSBGhostBrain>();
			if (ghostBrain.GetCurrentActionName() == GhostAction.Name.PartyPath)
			{
				// BUG: breaks on client cuz cast
				var partyPathAction = (QSBPartyPathAction)ghostBrain.GetCurrentAction();
				if (partyPathAction.hasReachedEndOfPath)
				{
					if (!partyPathAction.isMovingToFinalPosition)
					{
						var transform = __instance._numArrivedGhosts < __instance._ghostFinalDestinations.Length
							? __instance._ghostFinalDestinations[__instance._numArrivedGhosts].destinationTransform
							: __instance._ghostOverflowFinalDestinations[__instance._numArrivedGhosts % __instance._ghostOverflowFinalDestinations.Length].transform;
						partyPathAction.MoveToFinalPosition(transform.position);
						__instance._numArrivedGhosts++;
					}

					// BUG: only checks for host blocking the respawn?
					if (!__instance._respawnBlockTrigger.IsTrackingObject(Locator.GetPlayerDetector()))
					{
						__instance._dispatchedGhosts.QuickRemoveAt(i);
						ghostBrain.AttachedObject.transform.position = __instance._ghostSpawns[Random.Range(0, __instance._ghostSpawns.Length)].spawnTransform.position;
						ghostBrain.AttachedObject.transform.eulerAngles = Vector3.up * __instance._ghostSpawns[Random.Range(0, __instance._ghostSpawns.Length)].spawnTransform.eulerAngles.y;
						ghostBrain.TabulaRasa();
						partyPathAction.ResetPath();
						if (!__instance._disableGhostProxies && __instance._numEnabledGhostProxies < __instance._ghostFinalDestinations.Length)
						{
							if (__instance._ghostFinalDestinations[__instance._numEnabledGhostProxies].proxyGhost != null)
							{
								__instance._ghostFinalDestinations[__instance._numEnabledGhostProxies].proxyGhost.Reveal();
							}
							__instance._numEnabledGhostProxies++;
						}

						__instance._waitingGhosts.Add(ghostBrain.AttachedObject);
					}
				}
			}
		}

		if (__instance._waitingGhosts.Count > 0
			&& __instance._waitingGhosts[0].GetCurrentActionName() == GhostAction.Name.PartyPath
			&& (__instance._dispatchedGhosts.Count == 0 || Time.timeSinceLevelLoad > __instance._nextGhostDispatchTime))
		{
			DebugLog.DebugWrite($"Dispatch new ghost!");
			var ghostBrain2 = __instance._waitingGhosts[0].GetWorldObject<QSBGhostBrain>();
			var num = Random.Range(0, __instance._ghostSpawns.Length);
			ghostBrain2.AttachedObject.transform.position = __instance._ghostSpawns[num].spawnTransform.position;
			ghostBrain2.AttachedObject.transform.eulerAngles = Vector3.up * __instance._ghostSpawns[num].spawnTransform.eulerAngles.y;
			// BUG: breaks on client cuz cast
			((QSBPartyPathAction)ghostBrain2.GetCurrentAction()).StartFollowPath();
			__instance._ghostSpawns[num].spawnDoor.Open();
			__instance._ghostSpawns[num].spawnDoorTimer = Time.timeSinceLevelLoad + 4f;
			__instance._waitingGhosts.RemoveAt(0);
			__instance._lastDispatchedGhost = ghostBrain2.AttachedObject;
			__instance._dispatchedGhosts.Add(ghostBrain2.AttachedObject);
			__instance._nextGhostDispatchTime = Time.timeSinceLevelLoad + Random.Range(__instance._minGhostDispatchDelay, __instance._maxGhostDispatchDelay);
		}

		for (var j = 0; j < __instance._ghostSpawns.Length; j++)
		{
			if (__instance._ghostSpawns[j].spawnDoor.IsOpen() && Time.timeSinceLevelLoad > __instance._ghostSpawns[j].spawnDoorTimer)
			{
				__instance._ghostSpawns[j].spawnDoor.Close();
			}
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostPartyPathDirector.OnGhostIdentifyIntruder))]
	public static bool OnGhostIdentifyIntruder(GhostPartyPathDirector __instance, GhostBrain ghostBrain, GhostData ghostData)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}
}
