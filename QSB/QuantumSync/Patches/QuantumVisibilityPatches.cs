using HarmonyLib;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.QuantumSync.Patches
{
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
		public static bool ShapeVisibilityTracker_IsVisibleUsingCameraFrustum(ShapeVisibilityTracker __instance, ref bool __result)
		{
			__result = QuantumManager.IsVisibleUsingCameraFrustum(__instance, false).Item1;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ShapeVisibilityTracker), nameof(ShapeVisibilityTracker.IsVisible))]
		public static bool ShapeVisibilityTracker_IsVisible(ShapeVisibilityTracker __instance, ref bool __result)
		{
			__result = QuantumManager.IsVisible(__instance, false);
			return false;
		}

		// RendererVisibilityTracker patches - probably not needed as i don't think RendererVisibilityTracker is ever used?

		[HarmonyPrefix]
		[HarmonyPatch(typeof(RendererVisibilityTracker), nameof(RendererVisibilityTracker.IsVisibleUsingCameraFrustum))]
		public static bool RendererVisibilityTracker_IsVisibleUsingCameraFrustum(RendererVisibilityTracker __instance, ref bool __result, Renderer ____renderer, bool ____checkFrustumOcclusion)
		{
			__result = QSBPlayerManager.GetPlayersWithCameras()
					.Any(x => GeometryUtility.TestPlanesAABB(x.Camera.GetFrustumPlanes(), ____renderer.bounds))
				&& (!____checkFrustumOcclusion || QSBPlayerManager.GetPlayersWithCameras()
					.Any(x => !__instance.IsOccludedFromPosition(x.Camera.transform.position)));
			return false;
		}

		// VisibilityObject

		[HarmonyPrefix]
		[HarmonyPatch(typeof(VisibilityObject), nameof(VisibilityObject.CheckIllumination))]
		public static bool VisibilityObject_CheckIllumination(VisibilityObject __instance, ref bool __result, bool ____checkIllumination, Vector3 ____localIlluminationOffset, float ____illuminationRadius, Light[] ____lightSources)
		{
			if (!____checkIllumination)
			{
				__result = true;
				return false;
			}

			var point = __instance.transform.TransformPoint(____localIlluminationOffset);
			var tupleFlashlights = QSBPlayerManager.GetPlayerFlashlights();
			var localFlashlight = tupleFlashlights.Item1;
			var playerFlashlights = tupleFlashlights.Item2;

			// local player flashlight
			if (localFlashlight.CheckIlluminationAtPoint(point, ____illuminationRadius))
			{
				__result = true;
				return false;
			}

			// all other player flashlights
			if (playerFlashlights.Any(x => x.CheckIlluminationAtPoint(point, ____illuminationRadius)))
			{
				__result = true;
				return false;
			}

			// BUG : Implement checking for other probes!
			if (Locator.GetProbe() != null && Locator.GetProbe().IsLaunched() && Locator.GetProbe().CheckIlluminationAtPoint(point, ____illuminationRadius))
			{
				__result = true;
				return false;
			}

			// BUG : Implement checking for other player's thrusters!
			if (Locator.GetThrusterLightTracker().CheckIlluminationAtPoint(point, ____illuminationRadius))
			{
				__result = true;
				return false;
			}

			if (____lightSources != null)
			{
				foreach (var light in ____lightSources)
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
}
