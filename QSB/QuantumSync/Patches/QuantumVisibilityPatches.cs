using HarmonyLib;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.QuantumSync.Patches;

[HarmonyPatch]
public class QuantumVisibilityPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Shape), nameof(Shape.OnEnable))]
	public static void Shape_OnEnable(Shape __instance)
		=> __instance.RaiseEvent(nameof(__instance.OnShapeActivated), __instance);

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Shape), nameof(Shape.OnDisable))]
	public static void Shape_OnDisable(Shape __instance)
		=> __instance.RaiseEvent(nameof(__instance.OnShapeDeactivated), __instance);

	// ShapeVisibilityTracker patches

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ShapeVisibilityTracker), nameof(ShapeVisibilityTracker.IsVisibleUsingCameraFrustum))]
	public static bool ShapeVisibilityTracker_IsVisibleUsingCameraFrustum(ShapeVisibilityTracker __instance, out bool __result)
	{
		__result = QuantumManager.IsVisibleUsingCameraFrustum(__instance, false).FoundPlayers;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ShapeVisibilityTracker), nameof(ShapeVisibilityTracker.IsVisible))]
	public static bool ShapeVisibilityTracker_IsVisible(ShapeVisibilityTracker __instance, out bool __result)
	{
		__result = QuantumManager.IsVisible(__instance, false);
		return false;
	}

	// RendererVisibilityTracker patches - probably not needed as i don't think RendererVisibilityTracker is ever used?

	[HarmonyPrefix]
	[HarmonyPatch(typeof(RendererVisibilityTracker), nameof(RendererVisibilityTracker.IsVisibleUsingCameraFrustum))]
	public static bool RendererVisibilityTracker_IsVisibleUsingCameraFrustum(RendererVisibilityTracker __instance, out bool __result)
	{
		__result = QSBPlayerManager.GetPlayersWithCameras()
					   .Any(x => GeometryUtility.TestPlanesAABB(x.Camera.GetFrustumPlanes(), __instance._renderer.bounds))
				   && (!__instance._checkFrustumOcclusion || QSBPlayerManager.GetPlayersWithCameras()
					   .Any(x => !__instance.IsOccludedFromPosition(x.Camera.transform.position)));
		return false;
	}

	// VisibilityObject

	[HarmonyPrefix]
	[HarmonyPatch(typeof(VisibilityObject), nameof(VisibilityObject.CheckIllumination))]
	public static bool VisibilityObject_CheckIllumination(VisibilityObject __instance, out bool __result)
	{
		if (!__instance._checkIllumination)
		{
			__result = true;
			return false;
		}

		var point = __instance.transform.TransformPoint(__instance._localIlluminationOffset);
		var (localFlashlight, playerFlashlights) = QSBPlayerManager.GetPlayerFlashlights();

		if (localFlashlight.CheckIlluminationAtPoint(point, __instance._illuminationRadius))
		{
			__result = true;
			return false;
		}

		if (playerFlashlights.Any(x => x.CheckIlluminationAtPoint(point, __instance._illuminationRadius)))
		{
			__result = true;
			return false;
		}

		var (localProbe, playerProbes) = QSBPlayerManager.GetPlayerProbes();

		if (localProbe != null
			&& localProbe.IsLaunched()
			&& localProbe.CheckIlluminationAtPoint(point, __instance._illuminationRadius))
		{
			__result = true;
			return false;
		}

		if (playerProbes.Any(x => x.IsLaunched() && x.CheckIlluminationAtPoint(point, __instance._illuminationRadius)))
		{
			__result = true;
			return false;
		}

		if (QSBPlayerManager.GetThrusterLightTrackers()
			.Any(x => x.CheckIlluminationAtPoint(point, __instance._illuminationRadius)))
		{
			__result = true;
			return false;
		}

		if (__instance._lightSources != null)
		{
			foreach (var light in __instance._lightSources)
			{
				if (light.intensity > 0f && light.range > 0f)
				{
					__result = true;
					return false;
				}
			}
		}

		__result = false;
		return false;
	}
}