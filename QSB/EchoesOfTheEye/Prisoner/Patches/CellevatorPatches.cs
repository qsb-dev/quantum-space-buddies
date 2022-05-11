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
	[HarmonyPatch(nameof(PrisonCellElevator.CallElevatorToFloor))]
	public static void CallElevatorToFloor(PrisonCellElevator __instance, int floorIndex)
	{
		if (Remote)
		{
			return;
		}

		if (__instance._targetFloorIndex == floorIndex)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBPrisonCellElevator>()
			.SendMessage(new CellevatorCallMessage(floorIndex));
	}
}
