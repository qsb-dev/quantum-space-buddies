using HarmonyLib;
using QSB.ElevatorSync.Messages;
using QSB.ElevatorSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.Utility;
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

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Elevator), nameof(Elevator.AttachPlayerAndStartLift))]
	public static void Elevator_AttachPlayerAndStartLift(Elevator __instance)
	{
		// attach player to their current position instead of gliding them
		// to the attach point.
		var attachPoint = __instance._attachPoint;
		var qsbElevator = __instance.GetWorldObject<QSBElevator>();
		attachPoint.transform.position = Locator.GetPlayerTransform().position;

		// Runs when the lift/elevator is done moving.
		// Reset the position of the attach point.
		Delay.RunWhen(() => !__instance.enabled, () =>
		{
			attachPoint.transform.localPosition = qsbElevator.originalAttachPosition;
		});
	}
}
