using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.StationaryProbeLauncherSync.Messages;
using QSB.StationaryProbeLauncherSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.StationaryProbeLauncherSync.Patches;

[HarmonyPatch]
public class StationaryProbeLauncherPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(StationaryProbeLauncher), nameof(StationaryProbeLauncher.FinishExitSequence))]
	public static void StationaryProbeLauncher_FinishExitSequence(StationaryProbeLauncher __instance)
	{
		if (Remote)
		{
			return;
		}

		var qsbStationaryProbe = __instance.GetWorldObject<QSBStationaryProbeLauncher>();
		qsbStationaryProbe.SendMessage(new StationaryProbeLauncherMessage(false));
	}
}
