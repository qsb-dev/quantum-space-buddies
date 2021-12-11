﻿using HarmonyLib;
using QSB.Events;
using QSB.Patches;
using QSB.Tools.TranslatorTool.TranslationSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.Tools.TranslatorTool.TranslationSync.Patches
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
					QSBWorldSync.GetWorldFromUnity<QSBWallText>(__instance).ObjectId,
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
					QSBWorldSync.GetWorldFromUnity<QSBComputer>(__instance).ObjectId,
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
					QSBWorldSync.GetWorldFromUnity<QSBVesselComputer>(__instance).ObjectId,
					id);
			return true;
		}
	}
}
