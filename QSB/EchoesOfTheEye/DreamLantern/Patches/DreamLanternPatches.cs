using HarmonyLib;
using QSB.EchoesOfTheEye.DreamLantern.Messages;
using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.DreamLantern.Patches;

internal class DreamLanternPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternController), nameof(DreamLanternController.SetLit))]
	public static void SetLit(DreamLanternController __instance, bool lit)
	{
		if (Remote)
		{
			return;
		}

		if (__instance._lit == lit)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBDreamLantern>().SendMessage(new SetLitMessage(lit));
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternController), nameof(DreamLanternController.SetConcealed))]
	public static void SetConcealed(DreamLanternController __instance, bool concealed)
	{
		if (Remote)
		{
			return;
		}

		if (__instance._concealed == concealed)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBDreamLantern>().SendMessage(new SetConcealedMessage(concealed));
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternController), nameof(DreamLanternController.SetFocus))]
	public static void SetFocus(DreamLanternController __instance, float focus)
	{
		if (Remote)
		{
			return;
		}

		focus = Mathf.Clamp01(focus);
		if (OWMath.ApproxEquals(__instance._focus, focus))
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBDreamLantern>().SendMessage(new SetFocusMessage(focus));
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternController), nameof(DreamLanternController.SetRange))]
	public static void SetRange(DreamLanternController __instance, float minRange, float maxRange)
	{
		if (Remote)
		{
			return;
		}

		if (OWMath.ApproxEquals(__instance._minRange, minRange) && OWMath.ApproxEquals(__instance._maxRange, maxRange))
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBDreamLantern>().SendMessage(new SetRangeMessage(minRange, maxRange));
	}


	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternController), nameof(DreamLanternController.UpdateVisuals))]
	public static bool UpdateVisuals(DreamLanternController __instance)
	{
		// dont swap models if remote player picks up/drops item
		if (!Remote && __instance._dirtyFlag_heldByPlayer)
		{
			if (__instance._worldModelGroup != null)
			{
				__instance._worldModelGroup.SetActive(!__instance._heldByPlayer);
			}
			if (__instance._viewModelGroup != null)
			{
				__instance._viewModelGroup.SetActive(__instance._heldByPlayer);
			}
		}


		if (__instance._dirtyFlag_lit || __instance._dirtyFlag_flameStrength)
		{
			var vector = new Vector4(1f, 1f, 0f, 0f);
			vector.w = Mathf.Lerp(0.5f, 0f, __instance._flameStrength);
			for (var i = 0; i < __instance._flameRenderers.Length; i++)
			{
				__instance._flameRenderers[i].SetActivation(__instance._lit || __instance._flameStrength > 0f);
				__instance._flameRenderers[i].SetMaterialProperty(__instance._propID_MainTex_ST, vector);
			}
		}
		if (__instance._dirtyFlag_lensFlareStrength)
		{
			__instance._lensFlare.brightness = __instance._lensFlareStrength;
			__instance._lensFlare.enabled = __instance._lensFlareStrength > 0f;
		}
		if (__instance._dirtyFlag_focus)
		{
			var vector2 = new Vector3(0f, 0f, Mathf.Lerp(90f, 0f, __instance._focus));
			for (var j = 0; j < __instance._focuserPetals.Length; j++)
			{
				__instance._focuserPetals[j].localEulerAngles = __instance._focuserPetalsBaseEulerAngles[j] + vector2;
			}
		}
		if (__instance._dirtyFlag_concealment)
		{
			var vector3 = new Vector3(1f, Mathf.Lerp(0.5f, 1f, __instance._concealment), 1f);
			for (var k = 0; k < __instance._concealerRoots.Length; k++)
			{
				__instance._concealerRoots[k].localScale = Vector3.Scale(__instance._concealerRootsBaseScale[k], vector3);
			}
			for (var l = 0; l < __instance._concealerCovers.Length; l++)
			{
				__instance._concealerCovers[l].localPosition = Vector3.Lerp(__instance._concealerCoverTargets[l], __instance._concealerCoversStartPos[l], __instance._concealment);
				__instance._concealerCoversVMPrepass[l].localPosition = Vector3.Lerp(__instance._concealerCoverTargets[l], __instance._concealerCoversStartPos[l], __instance._concealment);
			}
		}
		if (__instance._dirtyFlag_flameStrength)
		{
			var flag = __instance._flameStrength > 0f;
			__instance._light.SetActivation(flag);
		}
		if (__instance._dirtyFlag_focus || __instance._dirtyFlag_flameStrength || __instance._dirtyFlag_range)
		{
			var num = Mathf.Lerp(__instance._minRange, __instance._maxRange, Mathf.Pow(__instance._focus, 5f)) * __instance._flameStrength;
			var num2 = Mathf.Lerp(__instance._maxAngle, __instance._minAngle, __instance._focus);
			__instance._light.range = num;
			__instance._light.GetLight().spotAngle = num2;
			__instance.SetDetectorPositionAndSize(num, num2);
		}
		if (__instance._grabbedByGhost)
		{
			var num3 = Mathf.MoveTowards(__instance._light.GetIntensity(), 1.2f, Time.deltaTime * 0.2f);
			__instance._light.SetIntensity(num3);
		}
		else if (__instance._dirtyFlag_socketed || __instance._dirtyFlag_grabbedByGhost)
		{
			__instance._light.SetIntensity(__instance._socketed ? 0f : 1f);
		}
		if (__instance._dirtyFlag_flameStrength)
		{
			for (var m = 0; m < __instance._flameLights.Length; m++)
			{
				__instance._flameLights[m].SetActivation(__instance._flameStrength > 0f);
				__instance._flameLights[m].SetIntensityScale(__instance._flameStrength);
			}
		}
		if ((__instance._dirtyFlag_focus || __instance._dirtyFlag_lit || __instance._dirtyFlag_concealed) && __instance._simLightConeUnfocused != null && __instance._simLightConeFocused != null)
		{
			var flag2 = __instance.IsFocused();
			__instance._simLightConeUnfocused.SetActive(__instance._lit && !__instance._concealed && !flag2);
			__instance._simLightConeFocused.SetActive(__instance._lit && !__instance._concealed && flag2);
		}
		__instance.ClearDirtyFlags();
		return false;
	}
}
