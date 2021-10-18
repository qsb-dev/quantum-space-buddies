using HarmonyLib;
using QSB.Events;
using QSB.Patches;
using QSB.TranslationSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.TranslationSync.Patches
{
	[HarmonyPatch]
	internal class SpiralPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiWallText), nameof(NomaiWallText.SetAsTranslated))]
		public static bool NomaiWallText_SetAsTranslated(NomaiWallText __instance, int id)
		{
			if (__instance.IsTranslated(id))
			{
				return true;
			}

			QSBEventManager.FireEvent(
					EventNames.QSBTextTranslated,
					NomaiTextType.WallText,
					QSBWorldSync.GetIdFromUnity<QSBWallText, NomaiWallText>(__instance),
					id);
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiComputer), nameof(NomaiWallText.SetAsTranslated))]
		public static bool NomaiComputer_SetAsTranslated(NomaiComputer __instance, int id)
		{
			if (__instance.IsTranslated(id))
			{
				return true;
			}

			QSBEventManager.FireEvent(
					EventNames.QSBTextTranslated,
					NomaiTextType.Computer,
					QSBWorldSync.GetIdFromUnity<QSBComputer, NomaiComputer>(__instance),
					id);
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiVesselComputer), nameof(NomaiWallText.SetAsTranslated))]
		public static bool NomaiVesselComputer_SetAsTranslated(NomaiVesselComputer __instance, int id)
		{
			if (__instance.IsTranslated(id))
			{
				return true;
			}

			QSBEventManager.FireEvent(
					EventNames.QSBTextTranslated,
					NomaiTextType.VesselComputer,
					QSBWorldSync.GetIdFromUnity<QSBVesselComputer, NomaiVesselComputer>(__instance),
					id);
			return true;
		}
	}
}
