using HarmonyLib;
using QSB.EchoesOfTheEye.DreamLantern.Messages;
using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.DreamLantern.Patches;

[HarmonyPatch(typeof(DreamLanternController))]
public class DreamLanternPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(DreamLanternController.SetLit))]
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

		var qsbDreamLantern = __instance.GetWorldObject<QSBDreamLanternController>();
		// ghost lanterns should only be controlled by the host
		if (qsbDreamLantern.IsGhostLantern && !QSBCore.IsHost)
		{
			return;
		}
		qsbDreamLantern.SendMessage(new SetLitMessage(lit));
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(DreamLanternController.SetConcealed))]
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

		var qsbDreamLantern = __instance.GetWorldObject<QSBDreamLanternController>();
		// ghost lanterns should only be controlled by the host
		if (qsbDreamLantern.IsGhostLantern && !QSBCore.IsHost)
		{
			return;
		}
		qsbDreamLantern.SendMessage(new SetConcealedMessage(concealed));
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(DreamLanternController.SetFocus))]
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

		var qsbDreamLantern = __instance.GetWorldObject<QSBDreamLanternController>();
		// ghost lanterns should only be controlled by the host
		if (qsbDreamLantern.IsGhostLantern && !QSBCore.IsHost)
		{
			return;
		}
		qsbDreamLantern.SendMessage(new SetFocusMessage(focus));
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(DreamLanternController.SetRange))]
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

		var qsbDreamLantern = __instance.GetWorldObject<QSBDreamLanternController>();
		// ghost lanterns should only be controlled by the host
		if (qsbDreamLantern.IsGhostLantern && !QSBCore.IsHost)
		{
			return;
		}
		qsbDreamLantern.SendMessage(new SetRangeMessage(minRange, maxRange));
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(DreamLanternController.UpdateVisuals))]
	public static bool UpdateVisuals(DreamLanternController __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var worldObject = __instance.GetWorldObject<QSBDreamLanternController>();

		if (worldObject.IsGhostLantern || worldObject.DreamLanternItem._lanternType == DreamLanternType.Malfunctioning)
		{
			return true;
		}

		if (__instance._dirtyFlag_heldByPlayer)
		{
			var localHeldItem = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
			var heldByLocalPlayer = localHeldItem != null && (DreamLanternItem)localHeldItem == worldObject.DreamLanternItem;
			// Only change to VM group when the local player is holding, not remote players
			if (heldByLocalPlayer)
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
		}

		if (__instance._dirtyFlag_lit || __instance._dirtyFlag_flameStrength)
		{
			var vector = new Vector4(1f, 1f, 0f, 0f);
			vector.w = Mathf.Lerp(0.5f, 0f, __instance._flameStrength);
			foreach (var flame in __instance._flameRenderers)
			{
				flame.SetActivation(__instance._lit || __instance._flameStrength > 0f);
				flame.SetMaterialProperty(__instance._propID_MainTex_ST, vector);
			}
		}

		if (__instance._dirtyFlag_lensFlareStrength)
		{
			__instance._lensFlare.brightness = __instance._lensFlareStrength;
			__instance._lensFlare.enabled = __instance._lensFlareStrength > 0f;
		}

		if (__instance._dirtyFlag_focus)
		{
			var petalRotation = new Vector3(0f, 0f, Mathf.Lerp(90f, 0f, __instance._focus));
			for (var j = 0; j < __instance._focuserPetals.Length; j++)
			{
				__instance._focuserPetals[j].localEulerAngles = __instance._focuserPetalsBaseEulerAngles[j] + petalRotation;
				worldObject.NonVMFocuserPetals[j].localEulerAngles = __instance._focuserPetalsBaseEulerAngles[j] + petalRotation;
			}
		}

		if (__instance._dirtyFlag_concealment)
		{
			var rootScale = new Vector3(1f, Mathf.Lerp(0.5f, 1f, __instance._concealment), 1f);
			for (var k = 0; k < __instance._concealerRoots.Length; k++)
			{
				__instance._concealerRoots[k].localScale = Vector3.Scale(__instance._concealerRootsBaseScale[k], rootScale);
				worldObject.NonVMConcealerRoots[k].localScale = Vector3.Scale(__instance._concealerRootsBaseScale[k], rootScale);
			}

			for (var l = 0; l < __instance._concealerCovers.Length; l++)
			{
				__instance._concealerCovers[l].localPosition = Vector3.Lerp(__instance._concealerCoverTargets[l], __instance._concealerCoversStartPos[l], __instance._concealment);
				__instance._concealerCoversVMPrepass[l].localPosition = Vector3.Lerp(__instance._concealerCoverTargets[l], __instance._concealerCoversStartPos[l], __instance._concealment);
				worldObject.NonVMConcealerCovers[l].localPosition = Vector3.Lerp(__instance._concealerCoverTargets[l], __instance._concealerCoversStartPos[l], __instance._concealment);
			}
		}

		if (__instance._dirtyFlag_flameStrength)
		{
			var flameActive = __instance._flameStrength > 0f;
			__instance._light.SetActivation(flameActive);
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
			foreach (var light in __instance._flameLights)
			{
				light.SetActivation(__instance._flameStrength > 0f);
				light.SetIntensityScale(__instance._flameStrength);
			}
		}

		if ((__instance._dirtyFlag_focus || __instance._dirtyFlag_lit || __instance._dirtyFlag_concealed) && __instance._simLightConeUnfocused != null && __instance._simLightConeFocused != null)
		{
			var flag2 = __instance.IsFocused(0.9f);
			__instance._simLightConeUnfocused.SetActive(__instance._lit && !__instance._concealed && !flag2);
			__instance._simLightConeFocused.SetActive(__instance._lit && !__instance._concealed && flag2);
		}

		__instance.ClearDirtyFlags();
		return false;
	}

	#region flare stuff

	[HarmonyPrefix]
	[HarmonyPatch(nameof(DreamLanternController.Update))]
	public static bool Update(DreamLanternController __instance)
	{
		// mmm i love not using transpiler LOL cry about it

		var num = 0f;
		// we want player lanterns to also have flare so remote player lanterns have it
		if (__instance._lit && !__instance._concealed /*&& !__instance._heldByPlayer*/)
		{
			var vector = Locator.GetActiveCamera().transform.position - __instance._light.transform.position;
			var num2 = 1f;
			if (vector.sqrMagnitude > __instance._light.GetLight().range * __instance._light.GetLight().range)
			{
				num2 = 0f;
			}
			else if (Vector3.Angle(__instance._light.transform.forward, vector) > __instance._light.GetLight().spotAngle * 0.5f)
			{
				num2 = 0f;
			}
			num = Mathf.MoveTowards(__instance._lensFlare.brightness, __instance._origLensFlareBrightness * num2, Time.deltaTime * 4f);
		}
		if (__instance._lensFlareStrength != num)
		{
			__instance._lensFlareStrength = num;
			__instance._dirtyFlag_lensFlareStrength = true;
		}
		var num3 = 0f;
		var num4 = 0.1f;
		if (__instance._lit)
		{
			num3 = __instance._concealed ? 0f : 1f;
			if (Time.time - __instance._litTime <= 1f)
			{
				num4 = 1f;
			}
			else
			{
				num4 = __instance._concealed ? 0.2f : 0.5f;
			}
		}
		var num5 = Mathf.MoveTowards(__instance._flameStrength, num3, Time.deltaTime / num4);
		if (__instance._flameStrength != num5)
		{
			__instance._flameStrength = num5;
			__instance._dirtyFlag_flameStrength = true;
		}
		var num6 = Mathf.MoveTowards(__instance._concealment, __instance._concealed ? 1f : 0f, Time.deltaTime / (__instance._concealed ? 0.15f : 0.5f));
		if (__instance._concealment != num6)
		{
			__instance._concealment = num6;
			__instance._dirtyFlag_concealment = true;
		}
		__instance.UpdateVisuals();

		return false;
	}

	#endregion
}
