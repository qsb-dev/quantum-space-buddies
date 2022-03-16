using HarmonyLib;
using QSB.EchoesOfTheEye.EclipseElevators.Messages;
using QSB.EchoesOfTheEye.EclipseElevators.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.EclipseElevators.Patches;

internal class EclipseElevatorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ElevatorDestination), nameof(ElevatorDestination.OnPressInteract))]
	public static bool CallElevator(ElevatorDestination __instance)
	{
		__instance.GetWorldObject<QSBElevatorDestination>().SendMessage(new CallElevatorMessage());
		return true;
	}
}
