using QSB.Events;
using QSB.Patches;

namespace QSB.TranslationSync.Patches
{
	internal class SpiralPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			QSBCore.Helper.HarmonyHelper.AddPrefix<NomaiWallText>("SetAsTranslated", typeof(SpiralPatches), nameof(Wall_SetAsTranslated));
			QSBCore.Helper.HarmonyHelper.AddPrefix<NomaiComputer>("SetAsTranslated", typeof(SpiralPatches), nameof(Computer_SetAsTranslated));
			QSBCore.Helper.HarmonyHelper.AddPrefix<NomaiVesselComputer>("SetAsTranslated", typeof(SpiralPatches), nameof(VesselComputer_SetAsTranslated));
		}

		public override void DoUnpatches()
		{
			QSBCore.Helper.HarmonyHelper.Unpatch<NomaiWallText>("SetAsTranslated");
			QSBCore.Helper.HarmonyHelper.Unpatch<NomaiComputer>("SetAsTranslated");
			QSBCore.Helper.HarmonyHelper.Unpatch<NomaiVesselComputer>("SetAsTranslated");
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
					SpiralManager.Instance.GetId(__instance),
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
					SpiralManager.Instance.GetId(__instance),
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
					SpiralManager.Instance.GetId(__instance),
					id);
			return true;
		}
	}
}
