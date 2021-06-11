using QSB.Patches;

namespace QSB.Inputs.Patches
{
	class InputPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
			=> QSBCore.HarmonyHelper.AddPrefix<OWInput>("Update", typeof(InputPatches), nameof(OWInput_Update));

		public override void DoUnpatches()
			=> QSBCore.HarmonyHelper.Unpatch<OWInput>("Update");

		public static bool OWInput_Update()
			=> QSBInputManager.Instance.InputsEnabled;
	}
}
