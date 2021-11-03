using HarmonyLib;
using QSB.Events;
using QSB.Patches;

namespace QSB.SatelliteSync.Patches
{
	[HarmonyPatch]
	class SatelliteProjectorPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SatelliteSnapshotController), nameof(SatelliteSnapshotController.OnPressInteract))]
		public static bool UseProjector()
		{
			QSBEventManager.FireEvent(EventNames.QSBEnterSatelliteCamera);
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SatelliteSnapshotController), nameof(SatelliteSnapshotController.TurnOffProjector))]
		public static bool LeaveProjector()
		{
			QSBEventManager.FireEvent(EventNames.QSBExitSatelliteCamera);
			return true;
		}
	}
}
