using HarmonyLib;
using QSB.Events;
using QSB.Patches;
using UnityEngine;

namespace QSB.RoastingSync.Patches
{
	[HarmonyPatch]
	internal class RoastingPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		//public override void DoPatches()
		//{
		//	Prefix<RoastingStickController>(nameof(RoastingStickController.UpdateMarshmallowInput), nameof(RoastingStickController_UpdateMarshmallowInput));
		//	Prefix<Marshmallow>(nameof(Marshmallow.Burn), nameof(Marshmallow_Burn));
		//	Prefix<Marshmallow>(nameof(Marshmallow.Shrivel), nameof(Marshmallow_Shrivel));
		//	Prefix<Marshmallow>(nameof(Marshmallow.RemoveMallow), nameof(Marshmallow_RemoveMallow));
		//	Prefix<Marshmallow>(nameof(Marshmallow.SpawnMallow), nameof(Marshmallow_SpawnMallow));
		//}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Marshmallow), nameof(Marshmallow.SpawnMallow))]
		public static bool Marshmallow_SpawnMallow()
		{
			QSBEventManager.FireEvent(EventNames.QSBMarshmallowEvent, MarshmallowEventType.Replace);
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Marshmallow), nameof(Marshmallow.Burn))]
		public static bool Marshmallow_Burn(
			ref Marshmallow.MallowState ____mallowState,
			MeshRenderer ____fireRenderer,
			ref float ____toastedFraction,
			ref float ____initBurnTime,
			PlayerAudioController ____audioController)
		{
			if (____mallowState == Marshmallow.MallowState.Default)
			{
				____fireRenderer.enabled = true;
				____toastedFraction = 1f;
				____initBurnTime = Time.time;
				____mallowState = Marshmallow.MallowState.Burning;
				____audioController.PlayMarshmallowCatchFire();
				QSBEventManager.FireEvent(EventNames.QSBMarshmallowEvent, MarshmallowEventType.Burn);
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Marshmallow), nameof(Marshmallow.Shrivel))]
		public static bool Marshmallow_Shrivel(
			ref Marshmallow.MallowState ____mallowState,
			ref float ____initShrivelTime)
		{
			if (____mallowState == Marshmallow.MallowState.Burning)
			{
				____initShrivelTime = Time.time;
				____mallowState = Marshmallow.MallowState.Shriveling;
				QSBEventManager.FireEvent(EventNames.QSBMarshmallowEvent, MarshmallowEventType.Shrivel);
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Marshmallow), nameof(Marshmallow.RemoveMallow))]
		public static bool Marshmallow_RemoveMallow(
			ParticleSystem ____smokeParticles,
			MeshRenderer ____fireRenderer,
			MeshRenderer ____mallowRenderer,
			ref Marshmallow.MallowState ____mallowState,
			Marshmallow __instance)
		{
			____smokeParticles.Stop();
			____fireRenderer.enabled = false;
			____mallowRenderer.enabled = false;
			____mallowState = Marshmallow.MallowState.Gone;
			__instance.enabled = false;
			QSBEventManager.FireEvent(EventNames.QSBMarshmallowEvent, MarshmallowEventType.Remove);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(RoastingStickController), nameof(RoastingStickController.UpdateMarshmallowInput))]
		public static bool RoastingStickController_UpdateMarshmallowInput(
			float ____extendFraction,
			Marshmallow ____marshmallow,
			GameObject ____mallowBodyPrefab,
			Transform ____stickTransform,
			Campfire ____campfire,
			ref string ____promptText,
			ScreenPrompt ____mallowPrompt,
			ref bool ____showMallowPrompt,
			ref bool ____showRemovePrompt)
		{
			var changePromptText = false;
			var showRemovePrompt = false;
			var text = string.Empty;
			if (____extendFraction == 0f)
			{
				if (____marshmallow.IsEdible())
				{
					text = UITextLibrary.GetString(UITextType.RoastingEatPrompt);
					changePromptText = true;
					if (____marshmallow.IsBurned())
					{
						showRemovePrompt = true;
						if (OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.Roasting))
						{
							____marshmallow.Remove();
							Locator.GetPlayerAudioController().PlayMarshmallowToss();
							var spawnedMarshmallow = UnityEngine.Object.Instantiate<GameObject>(____mallowBodyPrefab, ____stickTransform.position, ____stickTransform.rotation);
							var rigidbody = spawnedMarshmallow.GetComponent<OWRigidbody>();
							rigidbody.SetVelocity(____campfire.GetAttachedOWRigidbody(false).GetPointVelocity(____stickTransform.position) + (____stickTransform.forward * 3f));
							rigidbody.SetAngularVelocity(____stickTransform.right * 10f);
							var burntColor = ____marshmallow.GetBurntColor();
							spawnedMarshmallow.GetComponentInChildren<MeshRenderer>().material.color = burntColor;
							QSBEventManager.FireEvent(EventNames.QSBMarshmallowEvent, MarshmallowEventType.Toss);
						}
					}

					if (OWInput.IsNewlyPressed(InputLibrary.interact, InputMode.Roasting) && ____marshmallow.IsEdible())
					{
						____marshmallow.Eat();
					}
				}
				else if (____marshmallow.GetState() == Marshmallow.MallowState.Burning)
				{
					text = UITextLibrary.GetString(UITextType.RoastingExtinguishPrompt);
					changePromptText = true;
					if (OWInput.IsNewlyPressed(InputLibrary.interact, InputMode.Roasting))
					{
						____marshmallow.Extinguish();
						QSBEventManager.FireEvent(EventNames.QSBMarshmallowEvent, MarshmallowEventType.Extinguish);
					}
				}
				else if (____marshmallow.GetState() == Marshmallow.MallowState.Gone)
				{
					text = UITextLibrary.GetString(UITextType.RoastingReplacePrompt);
					changePromptText = true;
					if (OWInput.IsNewlyPressed(InputLibrary.interact, InputMode.Roasting))
					{
						____marshmallow.SpawnMallow(true);
					}
				}

				if (changePromptText && ____promptText != text)
				{
					____promptText = text;
					____mallowPrompt.SetText(____promptText);
				}

				if (OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.Roasting))
				{
					____campfire.StopRoasting();
					return false;
				}
			}

			____showMallowPrompt = changePromptText;
			____showRemovePrompt = showRemovePrompt;

			return false;
		}
	}
}
