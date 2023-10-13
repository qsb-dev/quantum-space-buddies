using HarmonyLib;
using QSB.EchoesOfTheEye.DreamLantern.Messages;
using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.DreamLantern.Patches;

public class DreamLanternPatches : QSBPatch
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

		var qsbDreamLantern = __instance.GetWorldObject<QSBDreamLanternController>();
		// ghost lanterns should only be controlled by the host
		if (qsbDreamLantern.IsGhostLantern && !QSBCore.IsHost)
		{
			return;
		}
		qsbDreamLantern.SendMessage(new SetLitMessage(lit));
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

		var qsbDreamLantern = __instance.GetWorldObject<QSBDreamLanternController>();
		// ghost lanterns should only be controlled by the host
		if (qsbDreamLantern.IsGhostLantern && !QSBCore.IsHost)
		{
			return;
		}
		qsbDreamLantern.SendMessage(new SetConcealedMessage(concealed));
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

		var qsbDreamLantern = __instance.GetWorldObject<QSBDreamLanternController>();
		// ghost lanterns should only be controlled by the host
		if (qsbDreamLantern.IsGhostLantern && !QSBCore.IsHost)
		{
			return;
		}
		qsbDreamLantern.SendMessage(new SetFocusMessage(focus));
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

		var qsbDreamLantern = __instance.GetWorldObject<QSBDreamLanternController>();
		// ghost lanterns should only be controlled by the host
		if (qsbDreamLantern.IsGhostLantern && !QSBCore.IsHost)
		{
			return;
		}
		qsbDreamLantern.SendMessage(new SetRangeMessage(minRange, maxRange));
	}

	#region flare stuff

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternController), nameof(DreamLanternController.Awake))]
	public static void Awake(DreamLanternController __instance)
	{
		__instance._lensFlare.brightness = 0.5f; // ghost lanterns use this
		// also has more blue lens flare. keep it like that for gameplay or wtv
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternController), nameof(DreamLanternController.Update))]
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
