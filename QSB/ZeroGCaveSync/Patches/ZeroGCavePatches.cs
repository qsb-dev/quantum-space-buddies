using HarmonyLib;
using OWML.Utils;
using QSB.Messaging;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using QSB.ZeroGCaveSync.Messages;
using QSB.ZeroGCaveSync.WorldObjects;
using UnityEngine;

namespace QSB.ZeroGCaveSync.Patches;

[HarmonyPatch]
public class ZeroGCavePatches : QSBPatch
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
		var qsbSatelliteNode = __instance.GetWorldObject<QSBSatelliteNode>();
		if (__instance._repairFraction >= 1f)
		{
			qsbSatelliteNode
				.SendMessage(new SatelliteNodeRepairedMessage());
			__instance._damaged = false;
			var component = Locator.GetPlayerTransform().GetComponent<ReferenceFrameTracker>();
			if (component.GetReferenceFrame() == __instance._rfVolume.GetReferenceFrame())
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

			__instance.RaiseEvent(nameof(__instance.OnRepaired), __instance);
		}

		if (__instance._damageEffect != null)
		{
			__instance._damageEffect.SetEffectBlend(1f - __instance._repairFraction);
		}

		qsbSatelliteNode
			.SendMessage(new SatelliteNodeRepairTickMessage(__instance._repairFraction));
		return false;
	}
}