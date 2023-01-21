using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.QuantumSync.Messages;
using QSB.QuantumSync.WorldObjects;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.QuantumSync.Patches.Common;

[HarmonyPatch(typeof(QuantumShuffleObject))]
internal class QuantumShuffleObjectPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(QuantumShuffleObject.ChangeQuantumState))]
	public static bool ChangeQuantumState(
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
			var temp = __instance._indexList[i];
			__instance._indexList[i] = __instance._indexList[random];
			__instance._indexList[random] = temp;
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
}
