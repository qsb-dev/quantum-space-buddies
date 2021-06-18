using QSB.Patches;

namespace QSB.GeyserSync.Patches
{
	internal class GeyserPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

		public override void DoPatches() => Empty("GeyserController_Update");
	}
}
