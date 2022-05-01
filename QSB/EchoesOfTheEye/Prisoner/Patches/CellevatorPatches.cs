using HarmonyLib;
using QSB.EchoesOfTheEye.Prisoner.WorldObjects;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.Prisoner.Patches;

[HarmonyPatch(typeof(PrisonCellElevator))]
public class CellevatorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PrisonCellElevator.CallToTopFloor))]
	public static bool CallToTopFloor(PrisonCellElevator __instance)
	{
		__instance.GetWorldObject<QSBPrisonCellElevator>().CallToFloorIndex(1);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PrisonCellElevator.CallToBottomFloor))]
	public static bool CallToBottomFloor(PrisonCellElevator __instance)
	{
		__instance.GetWorldObject<QSBPrisonCellElevator>().CallToFloorIndex(0);
		return false;
	}
}
