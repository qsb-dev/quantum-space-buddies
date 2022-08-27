using HarmonyLib;
using QSB.Patches;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.WorldSync;

namespace QSB.Tools.ProbeLauncherTool.Patches;

[HarmonyPatch]
public class StationaryProbeLauncherPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(StationaryProbeLauncher), nameof(StationaryProbeLauncher.FinishExitSequence))]
	public static void StationaryProbeLauncher_FinishExitSequence(StationaryProbeLauncher __instance) =>
		__instance.GetWorldObject<QSBStationaryProbeLauncher>().OnLocalUseStateChanged(false);
}
