using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.RoastingSync.Messages;
using UnityEngine;

namespace QSB.RoastingSync.Patches;

[HarmonyPatch]
public class RoastingPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Marshmallow), nameof(Marshmallow.SpawnMallow))]
	public static bool Marshmallow_SpawnMallow()
	{
		new MarshmallowEventMessage(MarshmallowMessageType.Replace).Send();
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Marshmallow), nameof(Marshmallow.Burn))]
	public static bool Marshmallow_Burn(
		Marshmallow __instance
	)
	{
		if (__instance._mallowState == Marshmallow.MallowState.Default)
		{
			__instance._fireRenderer.enabled = true;
			__instance._toastedFraction = 1f;
			__instance._initBurnTime = Time.time;
			__instance._mallowState = Marshmallow.MallowState.Burning;
			__instance._audioController.PlayMarshmallowCatchFire();
			new MarshmallowEventMessage(MarshmallowMessageType.Burn).Send();
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Marshmallow), nameof(Marshmallow.Shrivel))]
	public static bool Marshmallow_Shrivel(
		Marshmallow __instance
	)
	{
		if (__instance._mallowState == Marshmallow.MallowState.Burning)
		{
			__instance._initShrivelTime = Time.time;
			__instance._mallowState = Marshmallow.MallowState.Shriveling;
			new MarshmallowEventMessage(MarshmallowMessageType.Shrivel).Send();
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Marshmallow), nameof(Marshmallow.RemoveMallow))]
	public static bool Marshmallow_RemoveMallow(
		Marshmallow __instance)
	{
		__instance._smokeParticles.Stop();
		__instance._fireRenderer.enabled = false;
		__instance._mallowRenderer.enabled = false;
		__instance._mallowState = Marshmallow.MallowState.Gone;
		__instance.enabled = false;
		new MarshmallowEventMessage(MarshmallowMessageType.Remove).Send();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(RoastingStickController), nameof(RoastingStickController.UpdateMarshmallowInput))]
	public static bool RoastingStickController_UpdateMarshmallowInput(
		RoastingStickController __instance
	)
	{
		var changePromptText = false;
		var showRemovePrompt = false;
		var text = string.Empty;
		if (__instance._extendFraction == 0f)
		{
			if (__instance._marshmallow.IsEdible())
			{
				text = UITextLibrary.GetString(UITextType.RoastingEatPrompt);
				changePromptText = true;
				if (__instance._marshmallow.IsBurned())
				{
					showRemovePrompt = true;
					if (OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.Roasting))
					{
						InputLibrary.cancel.ConsumeInput();
						__instance._marshmallow.Remove();
						Locator.GetPlayerAudioController().PlayMarshmallowToss();
						var spawnedMarshmallow = Object.Instantiate(__instance._mallowBodyPrefab, __instance._stickTransform.position, __instance._stickTransform.rotation);
						var rigidbody = spawnedMarshmallow.GetComponent<OWRigidbody>();
						rigidbody.SetVelocity(__instance._campfire.GetAttachedOWRigidbody().GetPointVelocity(__instance._stickTransform.position) + __instance._stickTransform.forward * 3f);
						rigidbody.SetAngularVelocity(__instance._stickTransform.right * 10f);
						var burntColor = __instance._marshmallow.GetBurntColor();
						spawnedMarshmallow.GetComponentInChildren<MeshRenderer>().material.color = burntColor;
						new MarshmallowEventMessage(MarshmallowMessageType.Toss).Send();
					}
				}

				if (OWInput.IsNewlyPressed(InputLibrary.interact, InputMode.Roasting) && __instance._marshmallow.IsEdible())
				{
					__instance._marshmallow.Eat();
				}
			}
			else if (__instance._marshmallow.GetState() == Marshmallow.MallowState.Burning)
			{
				text = UITextLibrary.GetString(UITextType.RoastingExtinguishPrompt);
				changePromptText = true;
				if (OWInput.IsNewlyPressed(InputLibrary.interact, InputMode.Roasting))
				{
					__instance._marshmallow.Extinguish();
					new MarshmallowEventMessage(MarshmallowMessageType.Extinguish).Send();
				}
			}
			else if (__instance._marshmallow.GetState() == Marshmallow.MallowState.Gone)
			{
				text = UITextLibrary.GetString(UITextType.RoastingReplacePrompt);
				changePromptText = true;
				if (OWInput.IsNewlyPressed(InputLibrary.interact, InputMode.Roasting))
				{
					__instance._marshmallow.SpawnMallow(true);
				}
			}

			if (changePromptText && __instance._promptText != text)
			{
				__instance._promptText = text;
				__instance._mallowPrompt.SetText(__instance._promptText);
			}

			if (OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.Roasting))
			{
				__instance._campfire.StopRoasting();
				return false;
			}
		}

		__instance._showMallowPrompt = changePromptText;
		__instance._showRemovePrompt = showRemovePrompt;

		return false;
	}
}