using HarmonyLib;
using QSB.EchoesOfTheEye.DreamLantern.Messages;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.DreamLantern.Patches;

internal class DreamLanternPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.Update))]
	public static bool UpdateReplacement(DreamLanternItem __instance)
	{
		var heldItem = QSBPlayerManager.LocalPlayer.HeldItem;

		if (heldItem == null)
		{
			return false;
		}

		if (heldItem is not QSBDreamLanternItem qsbLantern)
		{
			return false;
		}

		if (__instance != qsbLantern.AttachedObject)
		{
			return false;
		}

		var isHoldingItem = Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item);

		__instance._wasFocusing = __instance._focusing;
		__instance._focusing = OWInput.IsPressed(InputLibrary.toolActionPrimary, InputMode.Character) && Time.time > __instance._forceUnfocusTime + 1f && isHoldingItem;

		var concealActionPressed = OWInput.IsPressed(InputLibrary.toolActionSecondary, InputMode.Character) && isHoldingItem;
		if (concealActionPressed && !__instance._lanternController.IsConcealed())
		{
			Locator.GetPlayerAudioController().OnArtifactConceal();
			__instance._lanternController.SetConcealed(true);
			new DreamLanternStateMessage(DreamLanternActionType.CONCEAL, true).Send();
		}
		else if (!concealActionPressed && __instance._lanternController.IsConcealed())
		{
			Locator.GetPlayerAudioController().OnArtifactUnconceal();
			__instance._lanternController.SetConcealed(false);
			new DreamLanternStateMessage(DreamLanternActionType.CONCEAL).Send();
		}

		if (__instance._focusing != __instance._wasFocusing)
		{
			if (__instance._focusing)
			{
				Locator.GetPlayerAudioController().OnArtifactFocus();
			}
			else
			{
				Locator.GetPlayerAudioController().OnArtifactUnfocus();
			}
		}

		__instance.UpdateFocus();

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternController), nameof(DreamLanternController.MoveTowardFocus))]
	public static bool UpdateFocusReplacement(DreamLanternController __instance, float targetFocus, float rate)
	{
		var value = Mathf.MoveTowards(__instance._focus, targetFocus, rate * Time.deltaTime);

		if (__instance._focus == value)
		{
			__instance.SetFocus(value);
			return false;
		}

		__instance.SetFocus(value);
		new DreamLanternStateMessage(DreamLanternActionType.FOCUS, floatValue: value).Send();

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.SetLit))]
	public static void SetLit(DreamLanternItem __instance, bool lit)
	{
		if (Remote)
		{
			return;
		}

		if (__instance._lanternController.IsLit() == lit)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBDreamLanternItem>().SendMessage(new DreamLanternLitMessage(lit));
	}
}
