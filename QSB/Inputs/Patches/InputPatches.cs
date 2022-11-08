using HarmonyLib;
using QSB.Patches;

namespace QSB.Inputs.Patches;

[HarmonyPatch]
internal class InputPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AbstractCommands), nameof(AbstractCommands.Update))]
	public static bool AbstractCommands_Update(AbstractCommands __instance)
	{
		__instance.Consumed = false;
		__instance.WasActiveLastFrame = __instance.IsActiveThisFrame;
		__instance.IsActiveThisFrame = false;

		if (__instance.WasActiveLastFrame)
		{
			__instance.InputStartedTime = float.MaxValue;
		}

		return false;
	}
}