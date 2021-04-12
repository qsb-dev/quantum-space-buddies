using QSB.Events;
using QSB.Patches;

namespace QSB.ShipSync.Patches
{
	class ShipPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			QSBCore.HarmonyHelper.AddPrefix<HatchController>("OnPressInteract", typeof(ShipPatches), nameof(HatchController_OnPressInteract));
			QSBCore.HarmonyHelper.AddPrefix<HatchController>("OnEntry", typeof(ShipPatches), nameof(HatchController_OnEntry));
		}

		public override void DoUnpatches()
		{
			QSBCore.HarmonyHelper.Unpatch<HatchController>("OnPressInteract");
			QSBCore.HarmonyHelper.Unpatch<HatchController>("OnEntry");
		}

		public static bool HatchController_OnPressInteract()
		{
			QSBEventManager.FireEvent(EventNames.QSBHatchState, true);
			return true;
		}

		public static bool HatchController_OnEntry(GameObjectActivationTrigger hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				QSBEventManager.FireEvent(EventNames.QSBHatchState, false);
			}
			return true;
		}
	}
}
