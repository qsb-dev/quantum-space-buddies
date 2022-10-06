using HarmonyLib;
using QSB.EchoesOfTheEye.WineCellar.Messages;
using QSB.EchoesOfTheEye.WineCellar.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.WineCellar.Patches;

internal class WineCellarPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(WineCellarSwitch), nameof(WineCellarSwitch.OnPressInteract))]
	public static void OnPressInteract(WineCellarSwitch __instance)
	{
		if (Remote)
		{
			return;
		}

		var worldObject = __instance.GetWorldObject<QSBWineCellarSwitch>();
		worldObject.SendMessage(new WineCellarSwitchMessage());
	}
}
