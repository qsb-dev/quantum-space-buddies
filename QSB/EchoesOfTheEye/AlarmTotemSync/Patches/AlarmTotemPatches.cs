using HarmonyLib;
using QSB.EchoesOfTheEye.AlarmTotemSync.Messages;
using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Patches;

public class AlarmTotemPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmTotem), nameof(AlarmTotem.SetFaceOpen))]
	private static void SetFaceOpen(AlarmTotem __instance, bool open)
	{
		if (Remote)
		{
			return;
		}

		if (__instance._isFaceOpen == open)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBAlarmTotem>()
			.SendMessage(new SetFaceOpenMessage(open));
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmTotem), nameof(AlarmTotem.OnSectorOccupantAdded))]
	private static void OnSectorOccupantAdded(AlarmTotem __instance, SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player && QSBWorldSync.AllObjectsAdded)
		{
			__instance.GetWorldObject<QSBAlarmTotem>()
				.SendMessage(new SetEnabledMessage(true));
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmTotem), nameof(AlarmTotem.OnSectorOccupantRemoved))]
	private static void OnSectorOccupantRemoved(AlarmTotem __instance, SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player && QSBWorldSync.AllObjectsAdded)
		{
			__instance.GetWorldObject<QSBAlarmTotem>()
				.SendMessage(new SetEnabledMessage(false));
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmTotem), nameof(AlarmTotem.FixedUpdate))]
	private static bool FixedUpdate(AlarmTotem __instance)
	{
		var isPlayerVisible = __instance._isPlayerVisible;
		__instance._isPlayerVisible = __instance.CheckPlayerVisible();
		if (__instance._isPlayerVisible && !isPlayerVisible)
		{
			Locator.GetAlarmSequenceController().IncreaseAlarmCounter();
			__instance._simTotemMaterials[0] = __instance._simAlarmMaterial;
			__instance._simTotemRenderer.sharedMaterials = __instance._simTotemMaterials;
			__instance._simVisionConeRenderer.SetColor(__instance._simAlarmColor);
			if (__instance._isTutorialTotem)
			{
				GlobalMessenger.FireEvent("TutorialAlarmTotemTriggered");
			}

			if (QSBWorldSync.AllObjectsReady)
			{
				__instance.GetWorldObject<QSBAlarmTotem>()
					.SendMessage(new SetVisibleMessage(true));
			}
		}
		else if (isPlayerVisible && !__instance._isPlayerVisible)
		{
			Locator.GetAlarmSequenceController().DecreaseAlarmCounter();
			__instance._simTotemMaterials[0] = __instance._origSimEyeMaterial;
			__instance._simTotemRenderer.sharedMaterials = __instance._simTotemMaterials;
			__instance._simVisionConeRenderer.SetColor(__instance._simVisionConeRenderer.GetOriginalColor());
			__instance._pulseLightController.FadeTo(0f, 0.5f);

			if (QSBWorldSync.AllObjectsReady)
			{
				__instance.GetWorldObject<QSBAlarmTotem>()
					.SendMessage(new SetVisibleMessage(false));
			}
		}

		return false;
	}
}
