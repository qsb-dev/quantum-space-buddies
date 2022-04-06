using HarmonyLib;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
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
}
