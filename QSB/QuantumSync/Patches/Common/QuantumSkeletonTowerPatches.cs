using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.QuantumSync.Messages;
using QSB.QuantumSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.QuantumSync.Patches.Common;

[HarmonyPatch(typeof(QuantumSkeletonTower))]
internal class QuantumSkeletonTowerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(QuantumSkeletonTower.ChangeQuantumState))]
	public static bool ChangeQuantumState(QuantumSkeletonTower __instance, ref bool __result)
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
}
