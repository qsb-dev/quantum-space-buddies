using HarmonyLib;
using OWML.Common;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.QuantumSync.Messages;
using QSB.QuantumSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.QuantumSync.Patches;

[HarmonyPatch]
public class QuantumPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(QuantumSocketCollapseTrigger), nameof(QuantumSocketCollapseTrigger.OnTriggerEnter))]
	public static bool QuantumSocketCollapseTrigger_OnTriggerEnter() => false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(QuantumObject), nameof(QuantumObject.IsLockedByPlayerContact))]
	public static bool QuantumObject_IsLockedByPlayerContact(out bool __result, QuantumObject __instance)
	{
		var playersEntangled = QuantumManager.GetEntangledPlayers(__instance);
		__result = playersEntangled.Count() != 0 && __instance.IsIlluminated();
		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(QuantumObject), nameof(QuantumObject.SetIsQuantum))]
	public static void QuantumObject_SetIsQuantum(QuantumObject __instance)
	{
		if (QSBWorldSync.AllObjectsReady)
		{
			__instance.GetWorldObject<IQSBQuantumObject>().SendMessage(new SetIsQuantumMessage(__instance.IsQuantum()));
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SocketedQuantumObject), nameof(SocketedQuantumObject.ChangeQuantumState))]
	public static bool SocketedQuantumObject_ChangeQuantumState(
		SocketedQuantumObject __instance,
		ref bool __result,
		bool skipInstantVisibilityCheck)
	{
		if (QSBWorldSync.AllObjectsReady)
		{
			var socketedWorldObject = __instance.GetWorldObject<QSBSocketedQuantumObject>();
			if (socketedWorldObject.ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				return false;
			}
		}

		foreach (var socket in __instance._childSockets)
		{
			if (socket.IsOccupied())
			{
				__result = false;
				return false;
			}
		}

		if (__instance._socketList.Count <= 1)
		{
			DebugLog.ToConsole($"Error - Not enough quantum sockets in list for {__instance.name}!", MessageType.Error);
			__result = false;
			return false;
		}

		var list = new List<QuantumSocket>();
		foreach (var socket in __instance._socketList)
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

		if (__instance._recentlyObscuredSocket != null)
		{
			__instance.MoveToSocket(__instance._recentlyObscuredSocket);
			__instance._recentlyObscuredSocket = null;
			__result = true;
			return false;
		}

		var occupiedSocket = __instance._occupiedSocket;
		for (var i = 0; i < 20; i++)
		{
			var index = Random.Range(0, list.Count);
			__instance.MoveToSocket(list[index]);
			if (skipInstantVisibilityCheck)
			{
				__result = true;
				return false;
			}

			bool socketNotSuitable;
			var isSocketIlluminated = __instance.CheckIllumination();

			var playersEntangled = QuantumManager.GetEntangledPlayers(__instance);
			if (playersEntangled.Count() != 0)
			{
				// socket not suitable if illuminated
				socketNotSuitable = isSocketIlluminated;
			}
			else
			{
				var checkVisInstant = __instance.CheckVisibilityInstantly();
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

		__instance.MoveToSocket(occupiedSocket);
		__result = false;
		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(SocketedQuantumObject), nameof(SocketedQuantumObject.MoveToSocket))]
	public static void SocketedQuantumObject_MoveToSocket(SocketedQuantumObject __instance, QuantumSocket socket)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		if (socket == null)
		{
			DebugLog.ToConsole($"Error - Trying to move {__instance.name} to a null socket!", MessageType.Error);
			return;
		}

		var objectWorldObject = __instance.GetWorldObject<QSBSocketedQuantumObject>();
		var socketWorldObject = socket.GetWorldObject<QSBQuantumSocket>();

		if (objectWorldObject == null)
		{
			DebugLog.ToConsole($"Worldobject is null for {__instance.name}!");
			return;
		}

		if (objectWorldObject.ControllingPlayer != QSBPlayerManager.LocalPlayerId)
		{
			return;
		}

		objectWorldObject.SendMessage(new SocketStateChangeMessage(
			socketWorldObject.ObjectId,
			__instance.transform.localRotation));
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(QuantumShuffleObject), nameof(QuantumShuffleObject.ChangeQuantumState))]
	public static bool QuantumShuffleObject_ChangeQuantumState(
		QuantumShuffleObject __instance,
		ref bool __result)
	{
		QSBQuantumShuffleObject shuffleWorldObject = default;
		if (QSBWorldSync.AllObjectsReady)
		{
			shuffleWorldObject = __instance.GetWorldObject<QSBQuantumShuffleObject>();
			if (shuffleWorldObject.ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				return false;
			}
		}

		__instance._indexList.Clear();
		__instance._indexList = Enumerable.Range(0, __instance._localPositions.Length).ToList();
		for (var i = 0; i < __instance._indexList.Count; ++i)
		{
			var random = Random.Range(i, __instance._indexList.Count);
			(__instance._indexList[random], __instance._indexList[i]) = (__instance._indexList[i], __instance._indexList[random]);
		}

		for (var j = 0; j < __instance._shuffledObjects.Length; j++)
		{
			__instance._shuffledObjects[j].localPosition = __instance._localPositions[__instance._indexList[j]];
		}

		if (QSBWorldSync.AllObjectsReady)
		{
			shuffleWorldObject.SendMessage(new QuantumShuffleMessage(__instance._indexList.ToArray()));
			__result = true;
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MultiStateQuantumObject), nameof(MultiStateQuantumObject.Start))]
	public static bool MultiStateQuantumObject_Start(MultiStateQuantumObject __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var qsbObj = __instance.GetWorldObject<QSBMultiStateQuantumObject>();
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

		if (__instance._sector == null)
		{
			__instance.CheckEnabled();
		}

		if (__instance._collapseOnStart)
		{
			__instance.Collapse(true);
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MultiStateQuantumObject), nameof(MultiStateQuantumObject.ChangeQuantumState))]
	public static bool MultiStateQuantumObject_ChangeQuantumState(MultiStateQuantumObject __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var qsbObj = __instance.GetWorldObject<QSBMultiStateQuantumObject>();
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
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		if (!visible)
		{
			return;
		}

		var allMultiStates = QSBWorldSync.GetWorldObjects<QSBMultiStateQuantumObject>();
		var stateObject = __instance.GetWorldObject<QSBQuantumState>();
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
		owner.SendMessage(new MultiStateChangeMessage(stateIndex));
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(QuantumShrine), nameof(QuantumShrine.IsPlayerInDarkness))]
	public static bool QuantumShrine_IsPlayerInDarkness(QuantumShrine __instance, out bool __result)
	{
		foreach (var lamp in __instance._lamps)
		{
			if (lamp.intensity > 0f)
			{
				__result = false;
				return false;
			}
		}

		var playersInMoon = QSBPlayerManager.PlayerList.Where(x => x.IsInMoon).ToList();

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

		if (playersInMoon.Count == 0)
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
		__result = __instance._gate.GetOpenFraction() == 0f
				   && !__instance._isProbeInside
				   && Locator.GetThrusterLightTracker().GetLightRange() <= 0f;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(QuantumShrine), nameof(QuantumShrine.ChangeQuantumState))]
	public static bool QuantumShrine_ChangeQuantumState(QuantumShrine __instance)
	{
		var shrineWorldObject = __instance.GetWorldObject<QSBSocketedQuantumObject>();
		var isInControl = shrineWorldObject.ControllingPlayer == QSBPlayerManager.LocalPlayerId;
		return isInControl;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(QuantumMoon), nameof(QuantumMoon.CheckPlayerFogProximity))]
	public static bool QuantumMoon_CheckPlayerFogProximity(QuantumMoon __instance)
	{
		var playerDistance = Vector3.Distance(__instance.transform.position, Locator.GetPlayerCamera().transform.position);
		var fogOffset = (__instance._stateIndex != 5) ? 0f : __instance._eyeStateFogOffset;
		var distanceFromFog = playerDistance - (__instance._fogRadius + fogOffset);
		var fogAlpha = 0f;
		if (!__instance._isPlayerInside)
		{
			fogAlpha = Mathf.InverseLerp(__instance._fogThickness + __instance._fogRolloffDistance, __instance._fogThickness, distanceFromFog);
			if (distanceFromFog < 0f)
			{
				if (__instance.IsLockedByProbeSnapshot() || QuantumManager.IsVisibleUsingCameraFrustum((ShapeVisibilityTracker)__instance._visibilityTracker, true).FoundPlayers)
				{
					__instance._isPlayerInside = true;
					__instance.SetSurfaceState(__instance._stateIndex);
					Locator.GetShipLogManager().RevealFact(__instance._revealFactID);
					GlobalMessenger.FireEvent(OWEvents.PlayerEnterQuantumMoon);
				}
				else
				{
					__instance.Collapse(true);
				}
			}
		}
		else if (__instance._isPlayerInside)
		{
			fogAlpha = Mathf.InverseLerp(-__instance._fogThickness - __instance._fogRolloffDistance, -__instance._fogThickness, distanceFromFog);
			if (distanceFromFog >= 0f)
			{
				if (__instance._stateIndex != 5)
				{
					__instance._isPlayerInside = false;
					if (!__instance.IsLockedByProbeSnapshot() && !QuantumManager.IsVisibleUsingCameraFrustum((ShapeVisibilityTracker)__instance._visibilityTracker, true).FoundPlayers)
					{
						__instance.Collapse(true);
					}

					__instance.SetSurfaceState(-1);
					GlobalMessenger.FireEvent(OWEvents.PlayerExitQuantumMoon);
				}
				else
				{
					var vector = Locator.GetPlayerTransform().position - __instance.transform.position;
					Locator.GetPlayerBody().SetVelocity(__instance._moonBody.GetPointVelocity(Locator.GetPlayerTransform().position) - (vector.normalized * 5f));
					var d = 80f;
					Locator.GetPlayerBody().SetPosition(__instance.transform.position + (__instance._vortexReturnPivot.up * d));
					if (!Physics.autoSyncTransforms)
					{
						Physics.SyncTransforms();
					}

					var component = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>();
					component.SetDegreesY(component.GetMinDegreesY());
					__instance._vortexAudio.SetLocalVolume(0f);
					__instance._collapseToIndex = 1;
					__instance.Collapse(true);
				}
			}
		}

		__instance._playerFogBubble.SetFogAlpha(fogAlpha);
		__instance._shipLandingCamFogBubble.SetFogAlpha(fogAlpha);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(QuantumSkeletonTower), nameof(QuantumSkeletonTower.ChangeQuantumState))]
	public static bool QuantumSkeletonTower_ChangeQuantumState(QuantumSkeletonTower __instance, ref bool __result)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var qsbQuantumSkeletonTower = __instance.GetWorldObject<QSBQuantumSkeletonTower>();
		if (qsbQuantumSkeletonTower.ControllingPlayer != QSBPlayerManager.LocalPlayerId)
		{
			return false;
		}

		if (__instance._waitForPlayerToLookAtTower)
		{
			__result = false;
			return false;
		}

		if (__instance._index < __instance._towerSkeletons.Length)
		{
			for (var i = 0; i < __instance._pointingSkeletons.Length; i++)
			{
				if (__instance._pointingSkeletons[i].gameObject.activeInHierarchy &&
					(!__instance._pointingSkeletons[i].IsVisible() || !__instance._pointingSkeletons[i].IsIlluminated()))
				{
					__instance._pointingSkeletons[i].gameObject.SetActive(false);

					__instance._towerSkeletons[__instance._index].SetActive(true);
					__instance._index++;
					__instance._waitForPlayerToLookAtTower = true;
					qsbQuantumSkeletonTower.SendMessage(new MoveSkeletonMessage(i));
					__result = true;
					return false;
				}
			}
		}

		__result = false;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(QuantumObject), nameof(QuantumObject.OnProbeSnapshot))]
	public static bool OnProbeSnapshot()
	{
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(QuantumObject), nameof(QuantumObject.OnProbeSnapshotRemoved))]
	public static bool OnProbeSnapshotRemoved()
	{
		return false;
	}
}