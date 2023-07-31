using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.SatelliteSync.Messages;
using UnityEngine;

namespace QSB.SatelliteSync.Patches;

[HarmonyPatch]
public class SatelliteProjectorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(SatelliteSnapshotController), nameof(SatelliteSnapshotController.Awake))]
	public static void CreateNewRenderTexture(SatelliteSnapshotController __instance)
	{
		__instance._snapshotTexture = SatelliteProjectorManager.Instance.SatelliteCameraSnapshot;
		__instance._satelliteCamera.targetTexture = __instance._snapshotTexture;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SatelliteSnapshotController), nameof(SatelliteSnapshotController.OnPressInteract))]
	public static bool UseProjector()
	{
		new SatelliteProjectorMessage(true).Send();
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SatelliteSnapshotController), nameof(SatelliteSnapshotController.TurnOffProjector))]
	public static bool LeaveProjector()
	{
		new SatelliteProjectorMessage(false).Send();
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

		if (OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary))
		{
			new SatelliteProjectorSnapshotMessage(true).Send();
			__instance._satelliteCamera.transform.localEulerAngles = __instance._initCamLocalRot;
			__instance.RenderSnapshot();
			return false;
		}

		if (__instance._allowRearview && OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary))
		{
			new SatelliteProjectorSnapshotMessage(false).Send();
			__instance._satelliteCamera.transform.localEulerAngles = __instance._initCamLocalRot + new Vector3(0f, 180f, 0f);
			__instance.RenderSnapshot();
			return false;
		}

		if (OWInput.IsNewlyPressed(InputLibrary.cancel))
		{
			__instance.TurnOffProjector();
		}

		return false;
	}
}