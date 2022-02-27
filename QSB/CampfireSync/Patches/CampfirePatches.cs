using HarmonyLib;
using QSB.CampfireSync.Messages;
using QSB.CampfireSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.CampfireSync.Patches
{
	[HarmonyPatch]
	internal class CampfirePatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Campfire), nameof(Campfire.OnPressInteract))]
		public static bool LightCampfireEvent(Campfire __instance)
		{
			var qsbCampfire = __instance.GetWorldObject<QSBCampfire>();
			if (__instance._state == Campfire.State.LIT)
			{
				qsbCampfire.StartRoasting();
			}
			else
			{
				qsbCampfire.SetState(Campfire.State.LIT);
				qsbCampfire.SendMessage(new CampfireStateMessage(Campfire.State.LIT));
				Locator.GetFlashlight().TurnOff(false);
			}

			return false;
		}
	}
}