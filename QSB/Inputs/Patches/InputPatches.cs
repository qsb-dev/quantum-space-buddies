using HarmonyLib;
using QSB.Patches;

namespace QSB.Inputs.Patches
{
	[HarmonyPatch]
	internal class InputPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(OWInput), nameof(OWInput.Update))]
		public static bool OWInput_Update()
			=> QSBInputManager.Instance.InputsEnabled;
	}
}
