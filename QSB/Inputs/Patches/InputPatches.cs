using HarmonyLib;
using QSB.Patches;

namespace QSB.Inputs.Patches;

[HarmonyPatch]
public class InputPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AbstractCommands), nameof(AbstractCommands.Update))]
	public static bool AbstractCommands_Update(AbstractCommands __instance)
	{
		if (QSBInputManager.Instance.InputsEnabled)
		{
			return true;
		}

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