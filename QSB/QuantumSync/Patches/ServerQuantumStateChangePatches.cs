using QSB.Events;
using QSB.Patches;
using QSB.QuantumSync.WorldObjects;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
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
	}
}