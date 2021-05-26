using OWML.Common;
using QSB.Events;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QSB.QuantumSync.Patches
{
	public class ServerQuantumPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnServerClientConnect;

		public override void DoPatches()
			=> QSBCore.HarmonyHelper.AddPrefix<QuantumMoon>("ChangeQuantumState", typeof(ServerQuantumPatches), nameof(Moon_ChangeQuantumState));

		public override void DoUnpatches()
			=> QSBCore.HarmonyHelper.Unpatch<QuantumMoon>("ChangeQuantumState");

		public static bool Moon_ChangeQuantumState(
			QuantumMoon __instance,
			ref bool __result,
			bool skipInstantVisibilityCheck,
			bool ____isPlayerInside,
			bool ____hasSunCollapsed,
			float ____playerWarpTime,
			ref int ____stateIndex,
			ref int ____collapseToIndex,
			QuantumOrbit[] ____orbits,
			float ____sphereCheckRadius,
			VisibilityTracker ____visibilityTracker,
			OWRigidbody ____moonBody,
			ConstantForceDetector ____constantForceDetector,
			ref bool ____useInitialMotion,
			ref int ____lastStateIndex,
			ref int[] ____stateSkipCounts,
			AudioSignal ____quantumSignal,
			ReferenceFrameVolume ____referenceFrameVolume,
			GameObject[] ____deactivateAtEye
			)
		{
			var isVisibleOutput = QuantumManager.IsVisibleUsingCameraFrustum((ShapeVisibilityTracker)____visibilityTracker, skipInstantVisibilityCheck);
			//var moonVisible = isVisibleOutput.First;
			var moonVisiblePlayers = isVisibleOutput.Second;
			var inMoonPlayers = QSBPlayerManager.PlayerList.Where(x => x.IsInMoon);
			var inShrinePlayers = QSBPlayerManager.PlayerList.Where(x => x.IsInShrine);
			//var outMoonPlayers = QSBPlayerManager.PlayerList.Where(x => !x.IsInMoon);
			var outShrinePlayers = QSBPlayerManager.PlayerList.Where(x => !x.IsInShrine);
			var shrineLit = QuantumManager.Shrine.IsPlayerInDarkness();

			// If any of the players in the moon are not in the shrine
			if (inMoonPlayers.Any(x => !x.IsInShrine))
			{
				__result = false;
				return false;
			}

			// If any of the players outside the shrine can see the moon
			if (outShrinePlayers.Any(moonVisiblePlayers.Contains))
			{
				__result = false;
				return false;
			}

			// If there are players in the shrine and the shrine is not lit
			if(inShrinePlayers.Count() != 0 && !shrineLit)
			{
				__result = false;
				return false;
			}

			var flag = false;
			if (____isPlayerInside && ____hasSunCollapsed)
			{
				__result = false;
				return false;
			}
			if (Time.time - ____playerWarpTime < 1f)
			{
				__result = false;
				return false;
			}
			if (____stateIndex == 5 && ____isPlayerInside && !__instance.IsPlayerEntangled())
			{
				__result = false;
				return false;
			}
			for (var i = 0; i < 10; i++)
			{
				var stateIndex = (____collapseToIndex == -1) ? (int)__instance.GetType().GetMethod("GetRandomStateIndex", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null) : ____collapseToIndex;
				var orbitIndex = -1;
				for (var j = 0; j < ____orbits.Length; j++)
				{
					if (____orbits[j].GetStateIndex() == stateIndex)
					{
						orbitIndex = j;
						break;
					}
				}
				if (orbitIndex == -1)
				{
					DebugLog.ToConsole($"Error - QM failed to find orbit for state {stateIndex}", MessageType.Error);
				}
				var orbitRadius = (orbitIndex == -1) ? 10000f : ____orbits[orbitIndex].GetOrbitRadius();
				var owRigidbody = (orbitIndex == -1) ? Locator.GetAstroObject(AstroObject.Name.Sun).GetOWRigidbody() : ____orbits[orbitIndex].GetAttachedOWRigidbody();
				var onUnitSphere = UnityEngine.Random.onUnitSphere;
				if (stateIndex == 5)
				{
					onUnitSphere.y = 0f;
					onUnitSphere.Normalize();
				}
				var position = (onUnitSphere * orbitRadius) + owRigidbody.GetWorldCenterOfMass();
				if (!Physics.CheckSphere(position, ____sphereCheckRadius, OWLayerMask.physicalMask) || ____collapseToIndex != -1)
				{
					____visibilityTracker.transform.position = position;
					if (!Physics.autoSyncTransforms)
					{
						Physics.SyncTransforms();
					}
					if (__instance.IsPlayerEntangled() || !QuantumManager.IsVisibleUsingCameraFrustum((ShapeVisibilityTracker)____visibilityTracker, skipInstantVisibilityCheck).First)
					{
						____moonBody.transform.position = position;
						if (!Physics.autoSyncTransforms)
						{
							Physics.SyncTransforms();
						}
						____visibilityTracker.transform.localPosition = Vector3.zero;
						____constantForceDetector.AddConstantVolume(owRigidbody.GetAttachedGravityVolume(), true, true);
						var velocity = owRigidbody.GetVelocity();
						if (____useInitialMotion)
						{
							var initialMotion = owRigidbody.GetComponent<InitialMotion>();
							velocity = (initialMotion == null) ? Vector3.zero : initialMotion.GetInitVelocity();
							____useInitialMotion = false;
						}
						var orbitAngle = UnityEngine.Random.Range(0, 360);
						____moonBody.SetVelocity(OWPhysics.CalculateOrbitVelocity(owRigidbody, ____moonBody, orbitAngle) + velocity);
						____lastStateIndex = ____stateIndex;
						____stateIndex = stateIndex;
						____collapseToIndex = -1;
						flag = true;
						for (var k = 0; k < ____stateSkipCounts.Length; k++)
						{
							____stateSkipCounts[k] = (k != ____stateIndex) ? (____stateSkipCounts[k] + 1) : 0;
						}
						QSBEventManager.FireEvent(EventNames.QSBMoonStateChange, stateIndex, onUnitSphere, orbitAngle);
						break;
					}
					____visibilityTracker.transform.localPosition = Vector3.zero;
				}
				else
				{
					DebugLog.ToConsole("Warning - Quantum moon orbit position occupied! Aborting collapse.", MessageType.Warning);
				}
			}
			if (flag)
			{
				if (____isPlayerInside)
				{
					__instance.GetType().GetMethod("SetSurfaceState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { ____stateIndex });
				}
				else
				{
					__instance.GetType().GetMethod("SetSurfaceState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { -1 });
					____quantumSignal.SetSignalActivation(____stateIndex != 5, 2f);
				}
				____referenceFrameVolume.gameObject.SetActive(____stateIndex != 5);
				____moonBody.SetIsTargetable(____stateIndex != 5);
				for (var l = 0; l < ____deactivateAtEye.Length; l++)
				{
					____deactivateAtEye[l].SetActive(____stateIndex != 5);
				}
				GlobalMessenger<OWRigidbody>.FireEvent("QuantumMoonChangeState", ____moonBody);
				__result = true;
				return false;
			}
			__result = false;
			return false;
		}
	}
}