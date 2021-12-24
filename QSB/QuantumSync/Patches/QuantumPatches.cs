using HarmonyLib;
using OWML.Common;
using QSB.Events;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Player.Messages;
using QSB.QuantumSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QSB.QuantumSync.Patches
{
	[HarmonyPatch]
	public class QuantumPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QuantumObject), nameof(QuantumObject.IsLockedByPlayerContact))]
		public static bool QuantumObject_IsLockedByPlayerContact(ref bool __result, QuantumObject __instance)
		{
			var playersEntangled = QuantumManager.GetEntangledPlayers(__instance);
			__result = playersEntangled.Count() != 0 && __instance.IsIlluminated();
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SocketedQuantumObject), nameof(SocketedQuantumObject.ChangeQuantumState))]
		public static bool SocketedQuantumObject_ChangeQuantumState(
			SocketedQuantumObject __instance,
			ref bool __result,
			bool skipInstantVisibilityCheck,
			List<QuantumSocket> ____childSockets,
			List<QuantumSocket> ____socketList,
			ref QuantumSocket ____recentlyObscuredSocket,
			QuantumSocket ____occupiedSocket)
		{
			if (WorldObjectManager.AllObjectsReady)
			{
				var socketedWorldObject = QSBWorldSync.GetWorldFromUnity<QSBSocketedQuantumObject>(__instance);
				if (socketedWorldObject.ControllingPlayer != QSBPlayerManager.LocalPlayerId)
				{
					return false;
				}
			}

			foreach (var socket in ____childSockets)
			{
				if (socket.IsOccupied())
				{
					__result = false;
					return false;
				}
			}

			if (____socketList.Count <= 1)
			{
				DebugLog.ToConsole($"Error - Not enough quantum sockets in list for {__instance.name}!", MessageType.Error);
				__result = false;
				return false;
			}

			var list = new List<QuantumSocket>();
			foreach (var socket in ____socketList)
			{
				if (!socket.IsOccupied() && socket.IsActive())
				{
					list.Add(socket);
				}
			}

			if (list.Count == 0)
			{
				__result = false;
				return false;
			}

			if (____recentlyObscuredSocket != null)
			{
				__instance.GetType().GetMethod("MoveToSocket", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { ____recentlyObscuredSocket });
				____recentlyObscuredSocket = null;
				__result = true;
				return false;
			}

			var occupiedSocket = ____occupiedSocket;
			for (var i = 0; i < 20; i++)
			{
				var index = UnityEngine.Random.Range(0, list.Count);
				__instance.GetType().GetMethod("MoveToSocket", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { list[index] });
				if (skipInstantVisibilityCheck)
				{
					__result = true;
					return false;
				}

				bool socketNotSuitable;
				var isSocketIlluminated = (bool)__instance.GetType().GetMethod("CheckIllumination", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);

				var playersEntangled = QuantumManager.GetEntangledPlayers(__instance);
				if (playersEntangled.Count() != 0)
				{
					// socket not suitable if illuminated
					socketNotSuitable = isSocketIlluminated;
				}
				else
				{
					var checkVisInstant = (bool)__instance.GetType().GetMethod("CheckVisibilityInstantly", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);
					if (isSocketIlluminated)
					{
						// socket not suitable if object is visible
						socketNotSuitable = checkVisInstant;
					}
					else
					{
						// socket not suitable if player is inside object
						socketNotSuitable = playersEntangled.Any(x => __instance.CheckPointInside(x.CameraBody.transform.position));
					}
				}

				if (!socketNotSuitable)
				{
					__result = true;
					return false;
				}

				list.RemoveAt(index);
				if (list.Count == 0)
				{
					break;
				}
			}

			__instance.GetType().GetMethod("MoveToSocket", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { occupiedSocket });
			__result = false;
			return false;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SocketedQuantumObject), nameof(SocketedQuantumObject.MoveToSocket))]
		public static void SocketedQuantumObject_MoveToSocket(SocketedQuantumObject __instance, QuantumSocket socket)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return;
			}

			if (socket == null)
			{
				DebugLog.ToConsole($"Error - Trying to move {__instance.name} to a null socket!", MessageType.Error);
				return;
			}

			var objectWorldObject = QSBWorldSync.GetWorldFromUnity<QSBSocketedQuantumObject>(__instance);
			var socketWorldObject = QSBWorldSync.GetWorldFromUnity<QSBQuantumSocket>(socket);

			if (objectWorldObject == null)
			{
				DebugLog.ToConsole($"Worldobject is null for {__instance.name}!");
				return;
			}

			if (objectWorldObject.ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				return;
			}

			QSBEventManager.FireEvent(
					EventNames.QSBSocketStateChange,
					objectWorldObject.ObjectId,
					socketWorldObject.ObjectId,
					__instance.transform.localRotation);
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QuantumShuffleObject), nameof(QuantumShuffleObject.ChangeQuantumState))]
		public static bool QuantumShuffleObject_ChangeQuantumState(
			QuantumShuffleObject __instance,
			ref List<int> ____indexList,
			ref Vector3[] ____localPositions,
			ref Transform[] ____shuffledObjects,
			ref bool __result)
		{
			QSBQuantumShuffleObject shuffleWorldObject = default;
			if (WorldObjectManager.AllObjectsReady)
			{
				shuffleWorldObject = QSBWorldSync.GetWorldFromUnity<QSBQuantumShuffleObject>(__instance);
				if (shuffleWorldObject.ControllingPlayer != QSBPlayerManager.LocalPlayerId)
				{
					return false;
				}
			}

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

			if (WorldObjectManager.AllObjectsReady)
			{
				QSBEventManager.FireEvent(
					EventNames.QSBQuantumShuffle,
					shuffleWorldObject.ObjectId,
					____indexList.ToArray());
				__result = true;
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MultiStateQuantumObject), nameof(MultiStateQuantumObject.Start))]
		public static bool MultiStateQuantumObject_Start(MultiStateQuantumObject __instance, Sector ____sector, bool ____collapseOnStart)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return true;
			}

			var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBMultiStateQuantumObject>(__instance);
			if (qsbObj.ControllingPlayer == 0)
			{
				return true;
			}

			foreach (var state in qsbObj.QuantumStates)
			{
				if (!state.IsMeantToBeEnabled)
				{
					state.SetVisible(false);
				}
			}

			if (____sector == null)
			{
				__instance.CheckEnabled();
			}

			if (____collapseOnStart)
			{
				__instance.Collapse(true);
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MultiStateQuantumObject), nameof(MultiStateQuantumObject.ChangeQuantumState))]
		public static bool MultiStateQuantumObject_ChangeQuantumState(MultiStateQuantumObject __instance)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return true;
			}

			var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBMultiStateQuantumObject>(__instance);
			if (qsbObj.ControllingPlayer == 0 && qsbObj.CurrentState == -1)
			{
				return true;
			}

			var isInControl = qsbObj.ControllingPlayer == QSBPlayerManager.LocalPlayerId;
			return isInControl;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(QuantumState), nameof(QuantumState.SetVisible))]
		public static void QuantumState_SetVisible(QuantumState __instance, bool visible)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return;
			}

			if (!visible)
			{
				return;
			}

			var allMultiStates = QSBWorldSync.GetWorldObjects<QSBMultiStateQuantumObject>();
			var stateObject = QSBWorldSync.GetWorldFromUnity<QSBQuantumState>(__instance);
			var owner = allMultiStates.FirstOrDefault(x => x.QuantumStates.Contains(stateObject));
			if (owner == default)
			{
				DebugLog.ToConsole($"Error - Could not find QSBMultiStateQuantumObject for state {__instance.name}", MessageType.Error);
				return;
			}

			if (owner.ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				return;
			}

			var stateIndex = owner.QuantumStates.IndexOf(stateObject);
			QSBEventManager.FireEvent(
					EventNames.QSBMultiStateChange,
					owner.ObjectId,
					stateIndex);
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QuantumShrine), nameof(QuantumShrine.IsPlayerInDarkness))]
		public static bool QuantumShrine_IsPlayerInDarkness(ref bool __result, Light[] ____lamps, float ____fadeFraction, bool ____isProbeInside, NomaiGateway ____gate)
		{
			foreach (var lamp in ____lamps)
			{
				if (lamp.intensity > 0f)
				{
					__result = false;
					return false;
				}
			}

			var playersInMoon = QSBPlayerManager.PlayerList.Where(x => x.IsInMoon);

			if (playersInMoon.Any(player => !player.IsInShrine))
			{
				__result = false;
				return false;
			}

			if (playersInMoon.Any(player => player.FlashLight != null && player.FlashLight.FlashlightOn))
			{
				__result = false;
				return false;
			}

			if (playersInMoon.Count() == 0)
			{
				__result = false;
				return false;
			}

			if (QSBPlayerManager.LocalPlayer != null
				&& QSBPlayerManager.LocalPlayer.IsInShrine
				&& PlayerState.IsFlashlightOn())
			{
				__result = false;
				return false;
			}

			// BUG : make this *really* check for all players - check other probes and other jetpacks!
			__result = ____gate.GetOpenFraction() == 0f
				&& !____isProbeInside
				&& Locator.GetThrusterLightTracker().GetLightRange() <= 0f;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QuantumShrine), nameof(QuantumShrine.ChangeQuantumState))]
		public static bool QuantumShrine_ChangeQuantumState(QuantumShrine __instance)
		{
			var shrineWorldObject = QSBWorldSync.GetWorldFromUnity<QSBSocketedQuantumObject>(__instance);
			var isInControl = shrineWorldObject.ControllingPlayer == QSBPlayerManager.LocalPlayerId;
			return isInControl;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QuantumShrine), nameof(QuantumShrine.OnEntry))]
		public static bool QuantumShrine_OnEntry(
			GameObject hitObj,
			ref bool ____isPlayerInside,
			ref bool ____fading,
			OWLightController ____exteriorLightController,
			ref bool ____isProbeInside)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				____isPlayerInside = true;
				____fading = true;
				____exteriorLightController.FadeTo(0f, 1f);
				new EnterLeaveMessage(EnterLeaveType.EnterShrine).Send();
			}
			else if (hitObj.CompareTag("ProbeDetector"))
			{
				____isProbeInside = true;
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QuantumShrine), nameof(QuantumShrine.OnExit))]
		public static bool QuantumShrine_OnExit(
			GameObject hitObj,
			ref bool ____isPlayerInside,
			ref bool ____fading,
			OWLightController ____exteriorLightController,
			ref bool ____isProbeInside)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				____isPlayerInside = false;
				____fading = true;
				____exteriorLightController.FadeTo(1f, 1f);
				new EnterLeaveMessage(EnterLeaveType.ExitShrine).Send();
			}
			else if (hitObj.CompareTag("ProbeDetector"))
			{
				____isProbeInside = false;
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QuantumMoon), nameof(QuantumMoon.CheckPlayerFogProximity))]
		public static bool QuantumMoon_CheckPlayerFogProximity(
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
					if ((bool)__instance.GetType().GetMethod("IsLockedByProbeSnapshot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null) || QuantumManager.IsVisibleUsingCameraFrustum((ShapeVisibilityTracker)____visibilityTracker, true).Item1)
					{
						____isPlayerInside = true;
						__instance.GetType().GetMethod("SetSurfaceState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { ____stateIndex });
						Locator.GetShipLogManager().RevealFact(____revealFactID, true, true);
						QSBEventManager.FireEvent(EventNames.PlayerEnterQuantumMoon);
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
						if (!(bool)__instance.GetType().GetMethod("IsLockedByProbeSnapshot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null) && !QuantumManager.IsVisibleUsingCameraFrustum((ShapeVisibilityTracker)____visibilityTracker, true).Item1)
						{
							__instance.GetType().GetMethod("Collapse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { true });
						}

						__instance.GetType().GetMethod("SetSurfaceState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { -1 });
						QSBEventManager.FireEvent(EventNames.PlayerExitQuantumMoon);
					}
					else
					{
						var vector = Locator.GetPlayerTransform().position - __instance.transform.position;
						Locator.GetPlayerBody().SetVelocity(____moonBody.GetPointVelocity(Locator.GetPlayerTransform().position) - (vector.normalized * 5f));
						var d = 80f;
						Locator.GetPlayerBody().SetPosition(__instance.transform.position + (____vortexReturnPivot.up * d));
						if (!Physics.autoSyncTransforms)
						{
							Physics.SyncTransforms();
						}

						var component = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>();
						component.SetDegreesY(component.GetMinDegreesY());
						____vortexAudio.SetLocalVolume(0f);
						____collapseToIndex = 1;
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
