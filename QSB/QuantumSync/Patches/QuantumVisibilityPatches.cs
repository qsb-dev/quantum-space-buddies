using QSB.Patches;
using QSB.Player;
using QSB.Utility;
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
		}

		// ShapeVisibilityTracker patches

		public static bool ShapeIsVisibleUsingCameraFrustum(ShapeVisibilityTracker __instance, ref bool __result)
		{
			__result = __instance.gameObject.activeInHierarchy
				&& QSBPlayerManager.GetPlayerCameras()
					.Any(x => (bool)__instance.GetType()
						.GetMethod("IsInFrustum", BindingFlags.NonPublic | BindingFlags.Instance)
						.Invoke(__instance, new object[] { x.GetFrustumPlanes() }));
			return false;
		}

		public static bool ShapeIsVisible(ShapeVisibilityTracker __instance, ref bool __result)
		{
			__result = __instance.gameObject.activeInHierarchy
				&& __instance.IsVisibleUsingCameraFrustum()
				&& QSBPlayerManager.GetPlayerCameras()
					.Any(x => VisibilityOccluder.CanYouSee(__instance, x.mainCamera.transform.position));
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
	}
}
