using QSB.Patches;
using QSB.Player;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QSB.QuantumSync.Patches
{
	public class QuantumVisibilityPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			QSBCore.Helper.HarmonyHelper.AddPrefix<ShapeVisibilityTracker>("IsVisibleUsingCameraFrustum", typeof(QuantumVisibilityPatches), nameof(ShapeIsVisibleUsingCameraFrustum));
			QSBCore.Helper.HarmonyHelper.AddPrefix<ShapeVisibilityTracker>("IsVisible", typeof(QuantumVisibilityPatches), nameof(ShapeIsVisible));
			QSBCore.Helper.HarmonyHelper.AddPrefix<RendererVisibilityTracker>("IsVisibleUsingCameraFrustum", typeof(QuantumVisibilityPatches), nameof(RenderIsVisibleUsingCameraFrustum));
			QSBCore.Helper.HarmonyHelper.AddPrefix<VisibilityObject>("CheckIllumination", typeof(QuantumVisibilityPatches), nameof(CheckIllumination));
		}

		// ShapeVisibilityTracker patches

		public static bool ShapeIsVisibleUsingCameraFrustum(ShapeVisibilityTracker __instance, ref bool __result)
		{
			__result = QuantumManager.IsVisibleUsingCameraFrustum(__instance, false);
			return false;
		}

		public static bool ShapeIsVisible(ShapeVisibilityTracker __instance, ref bool __result)
		{
			__result = QuantumManager.IsVisible(__instance, false);
			return false;
		}

		// RendererVisibilityTracker patches

		public static bool RenderIsVisibleUsingCameraFrustum(RendererVisibilityTracker __instance, ref bool __result, Renderer ____renderer, bool ____checkFrustumOcclusion)
		{
			__result = QSBPlayerManager.GetPlayerCameras()
					.Any(x => GeometryUtility.TestPlanesAABB(x.GetFrustumPlanes(), ____renderer.bounds))
				&& (!____checkFrustumOcclusion || QSBPlayerManager.GetPlayerCameras()
					.Any(x => !(bool)__instance.GetType()
					.GetMethod("IsOccludedFromPosition", BindingFlags.NonPublic | BindingFlags.Instance)
					.Invoke(__instance, new object[] { x.transform.position })));
			return false;
		}

		// VisibilityObject

		public static bool CheckIllumination(VisibilityObject __instance, ref bool __result, bool ____checkIllumination, Vector3 ____localIlluminationOffset, float ____illuminationRadius, Light[] ____lightSources)
		{
			if (!____checkIllumination)
			{
				__result = true;
				return false;
			}
			var point = __instance.transform.TransformPoint(____localIlluminationOffset);
			var tupleFlashlights = QSBPlayerManager.GetPlayerFlashlights();
			var localFlashlight = tupleFlashlights.First;
			var playerFlashlights = tupleFlashlights.Second;

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

			// TODO : Implement checking for other probes!
			if (Locator.GetProbe() != null && Locator.GetProbe().IsLaunched() && Locator.GetProbe().CheckIlluminationAtPoint(point, ____illuminationRadius))
			{
				__result = true;
				return false;
			}

			// TODO : Implement checking for other player's thrusters!
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
