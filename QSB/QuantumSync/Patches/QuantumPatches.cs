using QSB.Events;
using QSB.Patches;
using QSB.Player;
using QSB.QuantumSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.QuantumSync.Patches
{
	public class QuantumPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			QSBCore.Helper.HarmonyHelper.AddPrefix<SocketedQuantumObject>("ChangeQuantumState", typeof(QuantumPatches), nameof(Socketed_ChangeQuantumState));
			QSBCore.Helper.HarmonyHelper.AddPostfix<SocketedQuantumObject>("MoveToSocket", typeof(QuantumPatches), nameof(Socketed_MoveToSocket));
			QSBCore.Helper.HarmonyHelper.AddPrefix<QuantumShuffleObject>("ChangeQuantumState", typeof(QuantumPatches), nameof(Shuffle_ChangeQuantumState));
			QSBCore.Helper.HarmonyHelper.AddPrefix<MultiStateQuantumObject>("ChangeQuantumState", typeof(QuantumPatches), nameof(MultiState_ChangeQuantumState));
			QSBCore.Helper.HarmonyHelper.AddPostfix<QuantumState>("SetVisible", typeof(QuantumPatches), nameof(QuantumState_SetVisible));
			QSBCore.Helper.HarmonyHelper.AddPrefix<QuantumShrine>("IsPlayerInDarkness", typeof(QuantumPatches), nameof(Shrine_IsPlayerInDarkness));
			QSBCore.Helper.HarmonyHelper.AddPrefix<QuantumShrine>("ChangeQuantumState", typeof(QuantumPatches), nameof(Shrine_ChangeQuantumState));
			QSBCore.Helper.HarmonyHelper.AddPrefix<QuantumShrine>("OnEntry", typeof(QuantumPatches), nameof(Shrine_OnEntry));
			QSBCore.Helper.HarmonyHelper.AddPrefix<QuantumShrine>("OnExit", typeof(QuantumPatches), nameof(Shrine_OnExit));
		}

		public override void DoUnpatches()
		{
			QSBCore.Helper.HarmonyHelper.Unpatch<SocketedQuantumObject>("ChangeQuantumState");
			QSBCore.Helper.HarmonyHelper.Unpatch<SocketedQuantumObject>("MoveToSocket");
			QSBCore.Helper.HarmonyHelper.Unpatch<QuantumShuffleObject>("ChangeQuantumState");
			QSBCore.Helper.HarmonyHelper.Unpatch<MultiStateQuantumObject>("ChangeQuantumState");
			QSBCore.Helper.HarmonyHelper.Unpatch<QuantumState>("SetVisible");
			QSBCore.Helper.HarmonyHelper.Unpatch<QuantumShrine>("IsPlayerInDarkness");
			QSBCore.Helper.HarmonyHelper.Unpatch<QuantumShrine>("ChangeQuantumState");
			QSBCore.Helper.HarmonyHelper.Unpatch<QuantumShrine>("OnEntry");
			QSBCore.Helper.HarmonyHelper.Unpatch<QuantumShrine>("OnExit");
		}

		public static bool Socketed_ChangeQuantumState(SocketedQuantumObject __instance)
			=> QSBWorldSync.GetWorldObject<QSBSocketedQuantumObject>(QuantumManager.Instance.GetId(__instance)).ControllingPlayer == QSBPlayerManager.LocalPlayerId;

		public static void Socketed_MoveToSocket(SocketedQuantumObject __instance, QuantumSocket socket)
		{
			var id = QuantumManager.Instance.GetId(__instance);
			var worldObject = QSBWorldSync.GetWorldObject<QSBSocketedQuantumObject>(id);
			if (worldObject == null)
			{
				DebugLog.ToConsole($"Worldobject is null for id {id}!");
				return;
			}

			if (worldObject.ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				return;
			}

			var objId = QuantumManager.Instance.GetId(__instance);
			var socketId = QuantumManager.Instance.GetId(socket);
			//DebugLog.DebugWrite($"{__instance.name} to socket {socketId}");
			QSBEventManager.FireEvent(
					EventNames.QSBSocketStateChange,
					objId,
					socketId,
					__instance.transform.localRotation);
		}

		public static bool Shuffle_ChangeQuantumState(
			QuantumShuffleObject __instance,
			ref List<int> ____indexList,
			ref Vector3[] ____localPositions,
			ref Transform[] ____shuffledObjects,
			ref bool __result)
		{
			if (QSBWorldSync.GetWorldObject<QSBQuantumShuffleObject>(QuantumManager.Instance.GetId(__instance)).ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				return false;
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
			//DebugLog.DebugWrite($"{__instance.name} shuffled.");
			QSBEventManager.FireEvent(
					EventNames.QSBQuantumShuffle,
					QuantumManager.Instance.GetId(__instance),
					____indexList.ToArray());
			__result = true;
			return false;
		}

		public static bool MultiState_ChangeQuantumState(MultiStateQuantumObject __instance)
		{
			var qsbObj = QSBWorldSync.GetWorldObject<QSBMultiStateQuantumObject>(QuantumManager.Instance.GetId(__instance));
			var isInControl = qsbObj.ControllingPlayer == QSBPlayerManager.LocalPlayerId;
			return isInControl;
		}

		public static void QuantumState_SetVisible(QuantumState __instance, bool visible)
		{
			if (!visible)
			{
				return;
			}
			var allMultiStates = QSBWorldSync.GetWorldObjects<QSBMultiStateQuantumObject>();
			var owner = allMultiStates.First(x => x.QuantumStates.Contains(__instance));
			//DebugLog.DebugWrite($"{owner.AttachedObject.name} controller is {owner.ControllingPlayer}");
			if (owner.ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				return;
			}
			//DebugLog.DebugWrite($"{owner.AttachedObject.name} to quantum state {Array.IndexOf(owner.QuantumStates, __instance)}");
			QSBEventManager.FireEvent(
					EventNames.QSBMultiStateChange,
					QuantumManager.Instance.GetId(owner.AttachedObject),
					Array.IndexOf(owner.QuantumStates, __instance));
		}

		public static bool Shrine_IsPlayerInDarkness(ref bool __result, Light[] ____lamps, float ____fadeFraction, bool ____isProbeInside, NomaiGateway ____gate)
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
			if (playersInMoon.Any(x => !x.IsInShrine)
				|| playersInMoon.Any(x => x.FlashLight != null && x.FlashLight.FlashlightOn)
				|| (QSBPlayerManager.LocalPlayer.IsInShrine && PlayerState.IsFlashlightOn())
				|| playersInMoon.Count() == 0);
			{
				__result = false;
				return false;
			}
			// TODO : make this *really* check for all players - check other probes and other jetpacks!
			__result = ____gate.GetOpenFraction() == 0f
				&& !____isProbeInside
				&& Locator.GetThrusterLightTracker().GetLightRange() <= 0f;
			return false;
		}

		public static bool Shrine_ChangeQuantumState(QuantumShrine __instance)
		{
			var isInControl = QSBWorldSync.GetWorldObject<QSBSocketedQuantumObject>(QuantumManager.Instance.GetId(__instance)).ControllingPlayer == QSBPlayerManager.LocalPlayerId;
			return isInControl;
		}

		public static bool Shrine_OnEntry(
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
				QSBEventManager.FireEvent(EventNames.QSBEnterShrine);
			}
			else if (hitObj.CompareTag("ProbeDetector"))
			{
				____isProbeInside = true;
			}
			return false;
		}

		public static bool Shrine_OnExit(
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
				QSBEventManager.FireEvent(EventNames.QSBExitShrine);
			}
			else if (hitObj.CompareTag("ProbeDetector"))
			{
				____isProbeInside = false;
			}
			return false;
		}
	}
}
