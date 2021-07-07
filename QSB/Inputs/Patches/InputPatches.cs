using QSB.Patches;

namespace QSB.Inputs.Patches
{
	class InputPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
			=> Prefix(nameof(OWInput_Update));

		public static bool OWInput_Update()
			=> QSBInputManager.Instance.InputsEnabled;
	}
}
