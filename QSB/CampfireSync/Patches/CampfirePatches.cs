using HarmonyLib;
using QSB.CampfireSync.Messages;
using QSB.CampfireSync.WorldObjects;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.CampfireSync.Patches;

[HarmonyPatch]
internal class CampfirePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Campfire), nameof(Campfire.OnPressInteract))]
	public static bool LightCampfireEvent(Campfire __instance)
	{
		var qsbCampfire = __instance.GetWorldObject<QSBCampfire>();

		if (__instance._state != Campfire.State.LIT)
		{
			__instance.SetState(Campfire.State.LIT, false);
			qsbCampfire.SendMessage(new CampfireStateMessage(Campfire.State.LIT));
			Locator.GetFlashlight().TurnOff(false);
			if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ItemType.SlideReel)
			{
				__instance.SetDropSlideReelMode(true);
			}

			return false;
		}

		if (__instance._dropSlideReelMode)
		{
			var slideReelItem = (SlideReelItem)Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
			Locator.GetToolModeSwapper().GetItemCarryTool().DropItemInstantly(__instance._sector, __instance._burnedSlideReelSocket);
			slideReelItem.Burn();
			slideReelItem.GetWorldObject<QSBSlideReelItem>().SendMessage(new BurnSlideReelMessage(qsbCampfire));
			__instance.SetDropSlideReelMode(false);
			__instance._hasBurnedSlideReel = true;
			__instance._oneShotAudio.PlayOneShot(AudioType.TH_Campfire_Ignite, 1f);
			return false;
		}

		__instance.StartRoasting();

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Campfire), nameof(Campfire.Update))]
	public static bool UpdateReplacement(Campfire __instance)
	{
		var targetLitFraction = 0f;
		switch (__instance._state)
		{
			case Campfire.State.UNLIT:
				targetLitFraction = 0f;
				break;
			case Campfire.State.LIT:
				targetLitFraction = 1f;
				break;
			case Campfire.State.SMOLDERING:
				targetLitFraction = 0.4f;
				break;
		}

		if (__instance._litFraction != targetLitFraction)
		{
			__instance.SetLitFraction(Mathf.MoveTowards(__instance._litFraction, targetLitFraction, Time.deltaTime));
		}

		if (__instance._canSleepHere)
		{
			__instance._sleepPrompt.SetVisibility(false);
			if (__instance._interactVolumeFocus && !__instance._isPlayerSleeping && !__instance._isPlayerRoasting && OWInput.IsInputMode(InputMode.Character))
			{
				__instance._sleepPrompt.SetVisibility(true);
				__instance._sleepPrompt.SetDisplayState(__instance.CanSleepHereNow() ? ScreenPrompt.DisplayState.Normal : ScreenPrompt.DisplayState.GrayedOut);
				if (OWInput.IsNewlyPressed(InputLibrary.interactSecondary, InputMode.All) && __instance.CanSleepHereNow())
				{
					__instance.StartSleeping();
				}
			}
		}

		if (__instance._isPlayerSleeping && Time.timeSinceLevelLoad > __instance._fastForwardStartTime)
		{
			__instance._wakePrompt.SetVisibility(OWInput.IsInputMode(InputMode.None) && Time.timeSinceLevelLoad - __instance._fastForwardStartTime > __instance.GetWakePromptDelay());
			if (__instance.ShouldWakeUp())
			{
				__instance.StopSleeping(false);
				return false;
			}
		}

		return false;
	}
}