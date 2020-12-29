using QSB.Events;
using QSB.Patches;

namespace QSB.SpiralSync.Patches
{
	internal class SpiralPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches() => QSBCore.Helper.HarmonyHelper.AddPrefix<NomaiWallText>("SetAsTranslated", typeof(SpiralPatches), nameof(Wall_SetAsTranslated));

		public static bool Wall_SetAsTranslated(NomaiWallText __instance, int id)
		{
			if (__instance.IsTranslated(id))
			{
				return true;
			}
			GlobalMessenger<NomaiTextType, int, int>
				.FireEvent(
					EventNames.QSBTextTranslated,
					NomaiTextType.WallText,
					SpiralManager.Instance.GetId(__instance),
					id);
			return true;
		}
	}
}
