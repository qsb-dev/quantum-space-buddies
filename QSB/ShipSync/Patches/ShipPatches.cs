using QSB.Events;
using QSB.Patches;

namespace QSB.ShipSync.Patches
{
	class ShipPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches() 
			=> QSBCore.HarmonyHelper.AddPrefix<HatchController>("OnPressInteract", typeof(ShipPatches), nameof(HatchController_OnPressInteract));

		public override void DoUnpatches() 
			=> QSBCore.HarmonyHelper.Unpatch<HatchController>("OnPressInteract");

		public static bool HatchController_OnPressInteract()
		{
			QSBEventManager.FireEvent(EventNames.QSBOpenHatch);
			return true;
		}
	}
}
