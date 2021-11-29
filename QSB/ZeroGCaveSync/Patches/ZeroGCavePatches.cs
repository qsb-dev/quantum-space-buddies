using HarmonyLib;
using QSB.Events;
using QSB.Patches;
using QSB.Utility;
using UnityEngine;

namespace QSB.ZeroGCaveSync.Patches
{
	[HarmonyPatch]
	internal class ZeroGCavePatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SatelliteNode), nameof(SatelliteNode.RepairTick))]
		public static bool SatelliteNode_RepairTick(SatelliteNode __instance)
		{
			if (!__instance._damaged)
			{
				return false;
			}

			__instance._repairFraction = Mathf.Clamp01(__instance._repairFraction + (Time.deltaTime / __instance._repairTime));
			if (__instance._repairFraction >= 1f)
			{
				QSBEventManager.FireEvent(EventNames.QSBSatelliteRepaired, __instance);
				__instance._damaged = false;
				var component = Locator.GetPlayerTransform().GetComponent<ReferenceFrameTracker>();
				if (component.GetReferenceFrame(true) == __instance._rfVolume.GetReferenceFrame())
				{
					component.UntargetReferenceFrame();
				}

				if (__instance._rfVolume != null)
				{
					__instance._rfVolume.gameObject.SetActive(false);
				}

				if (__instance._lanternLight != null)
				{
					__instance._lanternLight.color = __instance._lightRepairedColor;
				}

				if (__instance._lanternEmissiveRenderer != null)
				{
					__instance._lanternEmissiveRenderer.sharedMaterials.CopyTo(__instance._lanternMaterials, 0);
					__instance._lanternMaterials[__instance._lanternMaterialIndex] = __instance._lanternRepairedMaterial;
					__instance._lanternEmissiveRenderer.sharedMaterials = __instance._lanternMaterials;
				}

				__instance.RaiseEvent("OnRepaired", __instance);
			}

			if (__instance._damageEffect != null)
			{
				__instance._damageEffect.SetEffectBlend(1f - __instance._repairFraction);
			}

			QSBEventManager.FireEvent(EventNames.QSBSatelliteRepairTick, __instance, __instance._repairFraction);
			return false;
		}
	}
}
