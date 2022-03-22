using HarmonyLib;
using QSB.ElevatorSync.Messages;
using QSB.ElevatorSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.ElevatorSync.Patches;

[HarmonyPatch]
public class ElevatorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Elevator), nameof(Elevator.StartLift))]
	public static void Elevator_StartLift(Elevator __instance)
	{
		if (Remote)
		{
			return;
		}

		var isGoingUp = __instance._goingToTheEnd;
		var qsbElevator = __instance.GetWorldObject<QSBElevator>();
		qsbElevator.SendMessage(new ElevatorMessage(isGoingUp));
	}
}
