using HarmonyLib;
using QSB.Patches;

namespace QSB.Animation.Player.Patches;

[HarmonyPatch(typeof(PlayerAnimController))]
public class PlayerAnimControllerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

	/*
	 * These patches preserve layer weights between animatorcontroller changes.
	 * No idea if this is intended Unity behaviour,
	 * but when changing the controller the layer weights get
	 * reset back to 0 - even if their default is not 0.
	 */

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PlayerAnimController.OnPutOnSuit))]
	public static bool OnPutOnSuit(PlayerAnimController __instance)
	{
		var layerCount = __instance._animator.layerCount;
		var layerWeights = new float[layerCount];
		for (var i = 0; i < layerCount; i++)
		{
			layerWeights[i] = __instance._animator.GetLayerWeight(i);
		}

		__instance._animator.runtimeAnimatorController = __instance._baseAnimController;
		__instance._unsuitedGroup.SetActive(false);
		__instance._suitedGroup.SetActive(!PlayerState.InMapView());

		for (var i = 0; i < layerCount; i++)
		{
			__instance._animator.SetLayerWeight(i, layerWeights[i]);
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PlayerAnimController.OnRemoveSuit))]
	public static bool OnRemoveSuit(PlayerAnimController __instance)
	{
		var layerCount = __instance._animator.layerCount;
		var layerWeights = new float[layerCount];
		for (var i = 0; i < layerCount; i++)
		{
			layerWeights[i] = __instance._animator.GetLayerWeight(i);
		}

		__instance._animator.runtimeAnimatorController = __instance._unsuitedAnimOverride;
		__instance._unsuitedGroup.SetActive(!PlayerState.InMapView());
		__instance._suitedGroup.SetActive(false);

		for (var i = 0; i < layerCount; i++)
		{
			__instance._animator.SetLayerWeight(i, layerWeights[i]);
		}

		return false;
	}
}
