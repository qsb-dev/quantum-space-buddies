using HarmonyLib;
using QSB.EchoesOfTheEye.Prisoner.Messages;
using QSB.EchoesOfTheEye.Prisoner.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.Prisoner.Patches;

[HarmonyPatch(typeof(PrisonCellElevator))]
public class CellevatorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PrisonCellElevator.CallToTopFloor))]
	public static void CallToTopFloor(PrisonCellElevator __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBPrisonCellElevator>()
			.SendMessage(new CellevatorCallMessage(1));
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PrisonCellElevator.CallToBottomFloor))]
	public static void CallToBottomFloor(PrisonCellElevator __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBPrisonCellElevator>()
			.SendMessage(new CellevatorCallMessage(0));
	}
}
