using QSB.CampfireSync.WorldObjects;
using QSB.Events;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.CampfireSync.Patches
{
	internal class CampfirePatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches() => Prefix(nameof(Campfire_OnPressInteract));

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
