using HarmonyLib;
using OWML.Utils;
using QSB.Messaging;
using QSB.Patches;
using QSB.Tools.ProbeTool.Messages;
using QSB.Utility;
using UnityEngine;

namespace QSB.Tools.ProbeTool.Patches;

public class ProbeToolPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ProbeCamera), nameof(ProbeCamera.RotateHorizontal))]
	public static bool RotateHorizontal(ProbeCamera __instance, float degrees)
	{
		if (__instance._id != ProbeCamera.ID.Rotating)
		{
			Debug.LogWarning("Tried to rotate a non-rotating ProbeCamera!", __instance);
			return false;
		}

		__instance._cameraRotation.x += degrees;
		__instance.transform.parent.localRotation = __instance._origParentLocalRotation * Quaternion.AngleAxis(__instance._cameraRotation.x, Vector3.up);
		__instance.RaiseEvent(nameof(__instance.OnRotateCamera), __instance._cameraRotation);
		new RotateProbeMessage(RotationType.Horizontal, __instance._cameraRotation).Send();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ProbeCamera), nameof(ProbeCamera.RotateVertical))]
	public static bool RotateVertical(ProbeCamera __instance, float degrees)
	{
		if (__instance._id != ProbeCamera.ID.Rotating)
		{
			Debug.LogWarning("Tried to rotate a non-rotating ProbeCamera!", __instance);
			return false;
		}

		__instance._cameraRotation.y = Mathf.Clamp(__instance._cameraRotation.y + degrees, -90f, 0f);
		__instance.transform.localRotation = __instance._origLocalRotation * Quaternion.AngleAxis(__instance._cameraRotation.y, Vector3.right);
		__instance.RaiseEvent(nameof(__instance.OnRotateCamera), __instance._cameraRotation);
		new RotateProbeMessage(RotationType.Vertical, __instance._cameraRotation).Send();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ProbeCamera), nameof(ProbeCamera.ResetRotation))]
	public static bool ResetRotation(ProbeCamera __instance)
	{
		if (__instance._id == ProbeCamera.ID.Rotating)
		{
			__instance._cameraRotation = Vector2.zero;
			__instance.transform.localRotation = __instance._origLocalRotation;
			__instance.transform.parent.localRotation = __instance._origParentLocalRotation;
			new RotateProbeMessage(RotationType.Reset, __instance._cameraRotation).Send();
		}

		return false;
	}
}