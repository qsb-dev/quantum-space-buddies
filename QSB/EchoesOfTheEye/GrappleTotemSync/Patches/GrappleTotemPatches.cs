using HarmonyLib;
using QSB.EchoesOfTheEye.GrappleTotemSync.Messages;
using QSB.EchoesOfTheEye.GrappleTotemSync.WorldObjects;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.GrappleTotemSync.Patches;

public class GrappleTotemPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(LanternZoomPoint), nameof(LanternZoomPoint.OnDetectLight))]
	private static bool OnDetectLight(LanternZoomPoint __instance) =>
		!QSBWorldSync.AllObjectsReady ||
		__instance._lightSensor.GetWorldObject<QSBLightSensor>().IlluminatedByLocal;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(LanternZoomPoint), nameof(LanternZoomPoint.StartZoomIn))]
	private static void StartZoomIn(LanternZoomPoint __instance)
	{
		if (__instance._totemAnimator != null && QSBWorldSync.AllObjectsReady)
		{
			__instance.GetWorldObject<QSBGrappleTotem>()
				.SendMessage(new GrappleAnimateMessage());
		}
	}
}
