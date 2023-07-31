using HarmonyLib;
using QSB.EchoesOfTheEye.PictureFrameDoors.Messages;
using QSB.EchoesOfTheEye.PictureFrameDoors.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.PictureFrameDoors.Patches;

public class PictureFrameDoorInterfacePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PictureFrameDoorInterface), nameof(PictureFrameDoorInterface.ToggleOpenState))]
	public static void ToggleOpenState(PictureFrameDoorInterface __instance)
		=> __instance.GetWorldObject<IQSBPictureFrameDoor>().SendMessage(new PictureFrameDoorMessage(__instance._door.IsOpen()));
}
