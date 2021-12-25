using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.Tools.TranslatorTool.TranslationSync.Messages;
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

			QSBWorldSync.GetWorldFromUnity<QSBWallText>(__instance)
				.SendMessage(new WallTextTranslatedMessage(id));
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

			QSBWorldSync.GetWorldFromUnity<QSBComputer>(__instance)
				.SendMessage(new ComputerTranslatedMessage(id));
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

			QSBWorldSync.GetWorldFromUnity<QSBVesselComputer>(__instance)
				.SendMessage(new VesselComputerTranslatedMessage(id));
			return true;
		}
	}
}
