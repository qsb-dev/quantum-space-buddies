using QSB.Patches;

namespace QSB.TimeSync.Patches
{
	public class WakeUpPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

		public override void DoPatches()
			=> QSBCore.HarmonyHelper.EmptyMethod<OWInput>("OnStartOfTimeLoop");

		public override void DoUnpatches()
			=> QSBCore.HarmonyHelper.Unpatch<OWInput>("OnStartOfTimeLoop");
	}
}