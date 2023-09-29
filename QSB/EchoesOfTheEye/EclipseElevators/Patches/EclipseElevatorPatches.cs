using HarmonyLib;
using QSB.EchoesOfTheEye.EclipseElevators.Messages;
using QSB.EchoesOfTheEye.EclipseElevators.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.EclipseElevators.Patches;

public class EclipseElevatorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ElevatorDestination), nameof(ElevatorDestination.OnPressInteract))]
	public static void CallElevator(ElevatorDestination __instance) =>
		__instance.GetWorldObject<QSBElevatorDestination>()
			.SendMessage(new CallElevatorMessage());
}
