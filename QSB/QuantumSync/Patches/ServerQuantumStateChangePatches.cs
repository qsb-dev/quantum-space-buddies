using QSB.Events;
using QSB.Patches;
using QSB.Player;
using QSB.QuantumSync.WorldObjects;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QSB.QuantumSync.Patches
{
	public class ServerQuantumStateChangePatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnServerClientConnect;

		public override void DoPatches()
		{
			QSBCore.Helper.HarmonyHelper.AddPostfix<SocketedQuantumObject>("MoveToSocket", typeof(ServerQuantumStateChangePatches), nameof(Socketed_MoveToSocket));
			QSBCore.Helper.HarmonyHelper.AddPostfix<QuantumState>("SetVisible", typeof(ServerQuantumStateChangePatches), nameof(QuantumState_SetVisible));
			QSBCore.Helper.HarmonyHelper.AddPrefix<QuantumShuffleObject>("ChangeQuantumState", typeof(ServerQuantumStateChangePatches), nameof(Shuffle_ChangeQuantumState));
			QSBCore.Helper.HarmonyHelper.AddPrefix<QuantumMoon>("ChangeQuantumState", typeof(ServerQuantumStateChangePatches), nameof(Moon_ChangeQuantumState));
			QSBCore.Helper.HarmonyHelper.AddPrefix<QuantumMoon>("CheckPlayerFogProximity", typeof(ServerQuantumStateChangePatches), nameof(Moon_CheckPlayerFogProximity));
		}

		public static void Socketed_MoveToSocket(SocketedQuantumObject __instance, QuantumSocket socket)
		{
			var objId = QuantumManager.Instance.GetId(__instance);
			var socketId = QuantumManager.Instance.GetId(socket);
			GlobalMessenger<int, int, Quaternion>
				.FireEvent(
					EventNames.QSBSocketStateChange,
					objId,
					socketId,
					__instance.transform.localRotation);
		}

		public static void QuantumState_SetVisible(QuantumState __instance, bool visible)
		{
			if (!visible)
			{
				return;
			}
			var allMultiStates = QSBWorldSync.GetWorldObjects<QSBMultiStateQuantumObject>();
			var owner = allMultiStates.First(x => x.QuantumStates.Contains(__instance));
			GlobalMessenger<int, int>
				.FireEvent(
					EventNames.QSBMultiStateChange,
					QuantumManager.Instance.GetId(owner.AttachedObject),
					Array.IndexOf(owner.QuantumStates, __instance));
		}

		public static bool Shuffle_ChangeQuantumState(
			QuantumShuffleObject __instance,
			ref List<int> ____indexList,
			ref Vector3[] ____localPositions,
			ref Transform[] ____shuffledObjects,
			ref bool __result)
		{
			____indexList.Clear();
			____indexList = Enumerable.Range(0, ____localPositions.Length).ToList();
			for (var i = 0; i < ____indexList.Count; ++i)
			{
				var random = UnityEngine.Random.Range(i, ____indexList.Count);
				var temp = ____indexList[i];
				____indexList[i] = ____indexList[random];
				____indexList[random] = temp;
			}
			for (var j = 0; j < ____shuffledObjects.Length; j++)
			{
				____shuffledObjects[j].localPosition = ____localPositions[____indexList[j]];
			}
			GlobalMessenger<int, int[]>
				.FireEvent(
					EventNames.QSBQuantumShuffle,
					QuantumManager.Instance.GetId(__instance),
					____indexList.ToArray());
			__result = true;
			return false;
		}

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
			if (skipInstantVisibilityCheck)
			{
				if (IsVisibleUsingCameraFrustum((ShapeVisibilityTracker)____visibilityTracker, true))
				{
					__result = false;
					return false;
				}
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
					Debug.LogError("QUANTUM MOON FAILED TO FIND ORBIT FOR STATE " + stateIndex);
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
					if (__instance.IsPlayerEntangled() || !IsVisibleUsingCameraFrustum((ShapeVisibilityTracker)____visibilityTracker, skipInstantVisibilityCheck))
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
						GlobalMessenger<int, Vector3, int>.FireEvent(EventNames.QSBMoonStateChange, stateIndex, onUnitSphere, orbitAngle);
						break;
					}
					____visibilityTracker.transform.localPosition = Vector3.zero;
				}
				else
				{
					Debug.LogError("Quantum moon orbit position occupied! Aborting collapse.");
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

		private static bool IsVisibleUsingCameraFrustum(ShapeVisibilityTracker tracker, bool skipVisibilityCheck)
		{
			return returnValue = tracker.gameObject.activeInHierarchy
				&& QSBPlayerManager.GetPlayerCameras(!skipVisibilityCheck)
					.Any(x => (bool)tracker.GetType()
						.GetMethod("IsInFrustum", BindingFlags.NonPublic | BindingFlags.Instance)
						.Invoke(tracker, new object[] { x.GetFrustumPlanes() }));
		}

		public static bool Moon_CheckPlayerFogProximity(
			QuantumMoon __instance,
			int ____stateIndex,
			float ____eyeStateFogOffset,
			ref bool ____isPlayerInside,
			float ____fogRadius,
			float ____fogThickness,
			float ____fogRolloffDistance,
			string ____revealFactID,
			OWRigidbody ____moonBody,
			bool ____hasSunCollapsed,
			Transform ____vortexReturnPivot,
			OWAudioSource ____vortexAudio,
			ref int ____collapseToIndex,
			VisibilityTracker ____visibilityTracker,
			QuantumFogEffectBubbleController ____playerFogBubble,
			QuantumFogEffectBubbleController ____shipLandingCamFogBubble)
		{
			var playerDistance = Vector3.Distance(__instance.transform.position, Locator.GetPlayerCamera().transform.position);
			var fogOffset = (____stateIndex != 5) ? 0f : ____eyeStateFogOffset;
			var distanceFromFog = playerDistance - (____fogRadius + fogOffset);
			var fogAlpha = 0f;
			if (!____isPlayerInside)
			{
				fogAlpha = Mathf.InverseLerp(____fogThickness + ____fogRolloffDistance, ____fogThickness, distanceFromFog);
				if (distanceFromFog < 0f)
				{
					if ((bool)__instance.GetType().GetMethod("IsLockedByProbeSnapshot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null) || IsVisibleUsingCameraFrustum((ShapeVisibilityTracker)____visibilityTracker, true))
					{
						____isPlayerInside = true;
						__instance.GetType().GetMethod("SetSurfaceState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { ____stateIndex });
						Locator.GetShipLogManager().RevealFact(____revealFactID, true, true);
						GlobalMessenger.FireEvent("PlayerEnterQuantumMoon");
					}
					else
					{
						__instance.GetType().GetMethod("Collapse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { true });
					}
				}
			}
			else if (____isPlayerInside)
			{
				fogAlpha = Mathf.InverseLerp(-____fogThickness - ____fogRolloffDistance, -____fogThickness, distanceFromFog);
				if (distanceFromFog >= 0f)
				{
					if (____stateIndex != 5)
					{
						____isPlayerInside = false;
						if (!(bool)__instance.GetType().GetMethod("IsLockedByProbeSnapshot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null) && !IsVisibleUsingCameraFrustum((ShapeVisibilityTracker)____visibilityTracker, true))
						{
							__instance.GetType().GetMethod("Collapse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { true });
						}
						__instance.GetType().GetMethod("SetSurfaceState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { -1 });
						GlobalMessenger.FireEvent("PlayerExitQuantumMoon");
					}
					else
					{
						var vector = Locator.GetPlayerTransform().position - __instance.transform.position;
						Locator.GetPlayerBody().SetVelocity(____moonBody.GetPointVelocity(Locator.GetPlayerTransform().position) - vector.normalized * 5f);
						var d = (!____hasSunCollapsed) ? (____fogRadius - 1f) : 80f;
						Locator.GetPlayerBody().SetPosition(__instance.transform.position + (____vortexReturnPivot.up * d));
						if (!Physics.autoSyncTransforms)
						{
							Physics.SyncTransforms();
						}
						var component = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>();
						component.SetDegreesY(component.GetMinDegreesY());
						____vortexAudio.SetLocalVolume(0f);
						____collapseToIndex = 1;
						// TODO : Handle players exiting eye state when other players are still one eye state, etc...
						__instance.GetType().GetMethod("Collapse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { true });
					}
				}
			}
			____playerFogBubble.SetFogAlpha(fogAlpha);
			____shipLandingCamFogBubble.SetFogAlpha(fogAlpha);
			return false;
		}
	}
}