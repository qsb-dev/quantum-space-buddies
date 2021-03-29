using QSB.CampfireSync.WorldObjects;
using QSB.Events;
using QSB.Patches;
using QSB.WorldSync;
using System;

namespace QSB.CampfireSync.Patches
{
	class CampfirePatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches() => QSBCore.HarmonyHelper.AddPrefix<Campfire>("OnPressInteract", typeof(CampfirePatches), nameof(Campfire_OnPressInteract));
		public override void DoUnpatches() => QSBCore.HarmonyHelper.Unpatch<Campfire>("OnPressInteract");

		public static bool Campfire_OnPressInteract(Campfire __instance, Campfire.State ____state)
		{
			var qsbCampfire = QSBWorldSync.GetWorldFromUnity<QSBCampfire, Campfire>(__instance);
			if (____state == Campfire.State.LIT)
			{
				qsbCampfire.StartRoasting();
			}
			else
			{
				qsbCampfire.SetState(Campfire.State.LIT);
				QSBEventManager.FireEvent(EventNames.QSBCampfireState, qsbCampfire.ObjectId, Campfire.State.LIT);
				Locator.GetFlashlight().TurnOff(false);
			}
			return false;
		}
	}
}
