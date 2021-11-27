using HarmonyLib;
using OWML.Utils;
using QSB.ElevatorSync.WorldObjects;
using QSB.Events;
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
			var isGoingUp = __instance.GetValue<bool>("_goingToTheEnd");
			var id = QSBWorldSync.GetIdFromUnity<QSBElevator>(__instance);
			QSBEventManager.FireEvent(EventNames.QSBStartLift, id, isGoingUp);
		}
	}
}