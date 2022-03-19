using HarmonyLib;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.Ghosts.Patches;

[HarmonyPatch(typeof(GhostHotelDirector))]
internal class GhostHotelDirectorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(GhostDirector), nameof(GhostDirector.OnDestroy))]
	public static void GhostDirector_OnDestroy_Stub(object instance) { }

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostHotelDirector.OnDestroy))]
	public static bool OnDestroy(GhostHotelDirector __instance)
	{
		GhostDirector_OnDestroy_Stub(__instance);

		__instance._hotelProjector.OnProjectorExtinguished -= __instance.OnHotelProjectorExtinguished;
		__instance._bridgeProjector.OnProjectorLit -= __instance.OnBridgeProjectorLit;
		__instance._depthsVolume.OnEntry -= __instance.OnEnterDepths;
		__instance._depthsVolume.OnExit -= __instance.OnExitDepths;
		for (var i = 0; i < __instance._hotelDepthsGhosts.Length; i++)
		{
			__instance._hotelDepthsGhosts[i].GetWorldObject<QSBGhostBrain>().OnIdentifyIntruder -= GhostManager.CustomOnHotelDepthsGhostsIdentifiedIntruder;
		}

		return false;
	}
}
