using HarmonyLib;
using OWML.Utils;
using QSB.ElevatorSync.Messages;
using QSB.ElevatorSync.WorldObjects;
using QSB.Events;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.ElevatorSync.Patches
{
	[HarmonyPatch]
	public class ElevatorPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Elevator), nameof(Elevator.StartLift))]
		public static void Elevator_StartLift(Elevator __instance)
		{
			var isGoingUp = __instance._goingToTheEnd;
			var qsbElevator = QSBWorldSync.GetWorldFromUnity<QSBElevator>(__instance);
			qsbElevator.SendMessage(new ElevatorMessage(isGoingUp));
		}
	}
}