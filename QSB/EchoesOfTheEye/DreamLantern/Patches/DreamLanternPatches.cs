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
}
