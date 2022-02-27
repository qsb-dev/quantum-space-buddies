using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.QuantumSync.Messages;
using QSB.QuantumSync.WorldObjects;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.QuantumSync.Patches
{
	[HarmonyPatch]
	public class ServerQuantumPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnServerClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(EyeProxyQuantumMoon), nameof(EyeProxyQuantumMoon.ChangeQuantumState))]
		public static bool EyeProxyQuantumMoon_ChangeQuantumState(EyeProxyQuantumMoon __instance, ref bool __result, bool skipInstantVisibilityCheck)
		{
			if (!QSBWorldSync.AllObjectsReady)
			{
				return true;
			}

			var qsbEyeProxyQuantumMoon = __instance.GetWorldObject<QSBEyeProxyQuantumMoon>();
			if (TimeLoop.GetSecondsRemaining() > 0f && Random.value > 0.3f)
			{
				__instance._moonStateRoot.SetActive(false);
				qsbEyeProxyQuantumMoon.SendMessage(new EyeProxyMoonStateChangeMessage(false, -1f));
				__result = true;
				return false;
			}

			__instance._moonStateRoot.SetActive(true);
			for (var i = 0; i < 20; i++)
			{
				var angle = Random.Range(0f, 360f);
				__instance.transform.localEulerAngles = new Vector3(0f, angle, 0f);
				if (skipInstantVisibilityCheck || !__instance.CheckVisibilityInstantly())
				{
					qsbEyeProxyQuantumMoon.SendMessage(new EyeProxyMoonStateChangeMessage(true, angle));
					__result = true;
					return false;
				}
			}

			__result = true;
			return false;
		}

		/*
		 * This patch used to be different, but that one completely broke Solanum's NomaiTextLines.
		 * I have no idea why, and I fixed it by just writing this patch again.
		 * The patches did the same thing.
		 * ¯\_(ツ)_/¯
		 */

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QuantumMoon), nameof(QuantumMoon.ChangeQuantumState))]
		public static bool ChangeQuantumState(QuantumMoon __instance, bool skipInstantVisibilityCheck, out bool __result)
		{
			var foundNewPosition = false;

			var playersInQuantumMoon = QSBPlayerManager.PlayerList.Where(x => x.IsInMoon);
			var isVisibleOutput = QuantumManager.IsVisibleUsingCameraFrustum((ShapeVisibilityTracker)__instance._visibilityTracker, skipInstantVisibilityCheck);
			var shrineLit = QuantumManager.Shrine != null && QuantumManager.Shrine.IsPlayerInDarkness();
			var playersInQuantumShrine = QSBPlayerManager.PlayerList.Where(x => x.IsInShrine);
			var playersWhoCanSeeQuantumMoon = isVisibleOutput.Item2;
			var playersNotInQuantumShrine = QSBPlayerManager.PlayerList.Where(x => !x.IsInShrine);

			// If any of the players in the moon are not in the shrine
			if (playersInQuantumMoon.Any(x => !x.IsInShrine))
			{
				__result = false;
				return false;
			}

			// If any of the players outside the shrine can see the moon
			if (playersNotInQuantumShrine.Any(playersWhoCanSeeQuantumMoon.Contains))
			{
				__result = false;
				return false;
			}

			// If there are players in the shrine and the shrine is not lit
			if (playersInQuantumShrine.Count() != 0 && !shrineLit)
			{
				__result = false;
				return false;
			}

			var anyPlayerInQM = playersInQuantumMoon.Any();

			if (anyPlayerInQM && __instance._hasSunCollapsed)
			{
				__result = false;
				return false;
			}

			if (Time.time - __instance._playerWarpTime < 1f)
			{
				__result = false;
				return false;
			}

			if (__instance._stateIndex == 5 && anyPlayerInQM && !__instance.IsPlayerEntangled())
			{
				__result = false;
				return false;
			}

			for (var i = 0; i < 10; i++)
			{
				FindStateAndOrbit(__instance, out var stateIndex, out var orbitIndex);

				GetTargetPosition(__instance, stateIndex, orbitIndex, out var orbitRadius, out var bodyToOrbit, out var targetPosition, out var onUnitSphere);

				if (!Physics.CheckSphere(targetPosition, __instance._sphereCheckRadius, OWLayerMask.physicalMask) || __instance._collapseToIndex != -1)
				{
					__instance._visibilityTracker.transform.position = targetPosition;
					if (!Physics.autoSyncTransforms)
					{
						Physics.SyncTransforms();
					}

					if (skipInstantVisibilityCheck || anyPlayerInQM || !__instance.CheckVisibilityInstantly())
					{
						MoveMoon(__instance, targetPosition, bodyToOrbit, stateIndex, onUnitSphere, ref foundNewPosition);
						break;
					}

					__instance._visibilityTracker.transform.localPosition = Vector3.zero;
				}
				else
				{
					Debug.LogError("Quantum moon orbit position occupied! Aborting collapse.");
				}
			}

			if (foundNewPosition)
			{
				DealWithNewPosition(__instance);
				__result = true;
				return false;
			}

			__result = false;
			return false;
		}

		private static void GetTargetPosition(QuantumMoon __instance, int stateIndex, int orbitIndex, out float orbitRadius, out OWRigidbody bodyToOrbit, out Vector3 targetPosition, out Vector3 onUnitSphere)
		{
			orbitRadius = (orbitIndex != -1)
				? __instance._orbits[orbitIndex].GetOrbitRadius()
				: 10000f;

			bodyToOrbit = (orbitIndex != -1)
				? __instance._orbits[orbitIndex].GetAttachedOWRigidbody()
				: Locator.GetAstroObject(AstroObject.Name.Sun).GetOWRigidbody();

			onUnitSphere = UnityEngine.Random.onUnitSphere;

			if (stateIndex == 5)
			{
				onUnitSphere.y = 0f;
				onUnitSphere.Normalize();
			}

			targetPosition = (onUnitSphere * orbitRadius) + bodyToOrbit.GetWorldCenterOfMass();
		}

		private static void FindStateAndOrbit(QuantumMoon __instance, out int stateIndex, out int orbitIndex)
		{
			stateIndex = (__instance._collapseToIndex != -1)
				? __instance._collapseToIndex
				: __instance.GetRandomStateIndex();
			orbitIndex = -1;

			for (var j = 0; j < __instance._orbits.Length; j++)
			{
				if (__instance._orbits[j].GetStateIndex() == stateIndex)
				{
					orbitIndex = j;
					break;
				}
			}

			if (orbitIndex == -1)
			{
				Debug.LogError("QUANTUM MOON FAILED TO FIND ORBIT FOR STATE " + stateIndex);
			}
		}

		private static void MoveMoon(QuantumMoon __instance, Vector3 targetPosition, OWRigidbody bodyToOrbit, int stateIndex, Vector3 onUnitSphere, ref bool foundNewPosition)
		{
			__instance._moonBody.transform.position = targetPosition;
			if (!Physics.autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}

			__instance._visibilityTracker.transform.localPosition = Vector3.zero;
			__instance._constantForceDetector.AddConstantVolume(bodyToOrbit.GetAttachedGravityVolume(), true, true);
			var bodyVelocity = bodyToOrbit.GetVelocity();

			if (__instance._useInitialMotion)
			{
				var component = bodyToOrbit.GetComponent<InitialMotion>();
				bodyVelocity = (component != null)
					? component.GetInitVelocity()
					: Vector3.zero;
				__instance._useInitialMotion = false;
			}

			var orbitAngle = Random.Range(0, 360);
			__instance._moonBody.SetVelocity(OWPhysics.CalculateOrbitVelocity(bodyToOrbit, __instance._moonBody, orbitAngle) + bodyVelocity);
			__instance._useInitialMotion = false;
			__instance._lastStateIndex = __instance._stateIndex;
			__instance._stateIndex = stateIndex;
			__instance._collapseToIndex = -1;
			foundNewPosition = true;

			for (var k = 0; k < __instance._stateSkipCounts.Length; k++)
			{
				__instance._stateSkipCounts[k] = (k == __instance._stateIndex)
					? 0
					: (__instance._stateSkipCounts[k] + 1);
			}

			new MoonStateChangeMessage(stateIndex, onUnitSphere, orbitAngle).Send();
		}

		private static void DealWithNewPosition(QuantumMoon __instance)
		{
			if (__instance._isPlayerInside)
			{
				__instance.SetSurfaceState(__instance._stateIndex);
			}
			else
			{
				__instance.SetSurfaceState(-1);
				__instance._quantumSignal.SetSignalActivation(__instance._stateIndex != 5);
			}

			__instance._referenceFrameVolume.gameObject.SetActive(__instance._stateIndex != 5);
			__instance._moonBody.SetIsTargetable(__instance._stateIndex != 5);

			for (var l = 0; l < __instance._deactivateAtEye.Length; l++)
			{
				__instance._deactivateAtEye[l].SetActive(__instance._stateIndex != 5);
			}

			GlobalMessenger<OWRigidbody>.FireEvent("QuantumMoonChangeState", __instance._moonBody);
		}
	}
}