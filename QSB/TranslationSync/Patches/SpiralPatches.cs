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
			Prefix(nameof(NomaiWallText_SetAsTranslated));
			Prefix(nameof(NomaiComputer_SetAsTranslated));
			Prefix(nameof(NomaiVesselComputer_SetAsTranslated));
		}

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
