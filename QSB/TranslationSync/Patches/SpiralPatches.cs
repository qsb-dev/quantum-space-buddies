using QSB.Events;
using QSB.Patches;
using QSB.TranslationSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.TranslationSync.Patches
{
	internal class SpiralPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			QSBCore.HarmonyHelper.AddPrefix<NomaiWallText>("SetAsTranslated", typeof(SpiralPatches), nameof(Wall_SetAsTranslated));
			QSBCore.HarmonyHelper.AddPrefix<NomaiComputer>("SetAsTranslated", typeof(SpiralPatches), nameof(Computer_SetAsTranslated));
			QSBCore.HarmonyHelper.AddPrefix<NomaiVesselComputer>("SetAsTranslated", typeof(SpiralPatches), nameof(VesselComputer_SetAsTranslated));
		}

		public override void DoUnpatches()
		{
			QSBCore.HarmonyHelper.Unpatch<NomaiWallText>("SetAsTranslated");
			QSBCore.HarmonyHelper.Unpatch<NomaiComputer>("SetAsTranslated");
			QSBCore.HarmonyHelper.Unpatch<NomaiVesselComputer>("SetAsTranslated");
		}

		public static bool Wall_SetAsTranslated(NomaiWallText __instance, int id)
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

		public static bool Computer_SetAsTranslated(NomaiComputer __instance, int id)
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

		public static bool VesselComputer_SetAsTranslated(NomaiVesselComputer __instance, int id)
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
