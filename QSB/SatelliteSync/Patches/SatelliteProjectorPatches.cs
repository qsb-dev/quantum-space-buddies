using HarmonyLib;
using QSB.Events;
using QSB.Patches;
using UnityEngine;

namespace QSB.SatelliteSync.Patches
{
	[HarmonyPatch]
	internal class SatelliteProjectorPatches : QSBPatch
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

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SatelliteSnapshotController), nameof(SatelliteSnapshotController.Update))]
		public static bool UpdateReplacement(SatelliteSnapshotController __instance)
		{
			if (!OWInput.IsInputMode(InputMode.SatelliteCam))
			{
				return false;
			}

			if (OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary, InputMode.All))
			{
				QSBEventManager.FireEvent(EventNames.QSBSatelliteSnapshot, true);
				__instance._satelliteCamera.transform.localEulerAngles = __instance._initCamLocalRot;
				__instance.RenderSnapshot();
				return false;
			}

			if (__instance._allowRearview && OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary, InputMode.All))
			{
				QSBEventManager.FireEvent(EventNames.QSBSatelliteSnapshot, false);
				__instance._satelliteCamera.transform.localEulerAngles = __instance._initCamLocalRot + new Vector3(0f, 180f, 0f);
				__instance.RenderSnapshot();
				return false;
			}

			if (OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.All))
			{
				__instance.TurnOffProjector();
			}

			return false;
		}
	}
}
