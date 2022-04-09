using HarmonyLib;
using QSB.EchoesOfTheEye.Sarcophagus.Messages;
using QSB.EchoesOfTheEye.Sarcophagus.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.Sarcophagus.Patches;

public class SarcophagusPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SarcophagusController), nameof(SarcophagusController.OnPressInteract))]
	private static void OnPressInteract(SarcophagusController __instance)
	{
		if (Remote)
		{
			__instance.GetComponent<GearInterface>().OnPressInteract();
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBSarcophagus>()
			.SendMessage(new OpenMessage());
	}
}
