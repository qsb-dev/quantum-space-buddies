using QSB.Patches;

namespace QSB.GeyserSync.Patches
{
	internal class GeyserPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

		public override void DoPatches() => QSBCore.HarmonyHelper.EmptyMethod<GeyserController>("Update");
		public override void DoUnpatches() => QSBCore.HarmonyHelper.Unpatch<GeyserController>("Update");
	}
}
