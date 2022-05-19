using HarmonyLib;
using QSB.Patches;
using QSB.Player;
using QSB.ShipSync.TransformSync;
using UnityEngine;

namespace QSB.ShipSync.Patches;

[HarmonyPatch(typeof(ThrusterFlameController))]
internal class ShipFlamePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ThrusterFlameController.GetThrustFraction))]
	public static bool GetThrustFraction(ThrusterFlameController __instance, ref float __result)
	{
		if (!ShipThrusterManager.ShipFlameControllers.Contains(__instance))
		{
			return true;
		}

		if (!__instance._thrusterModel.IsThrusterBankEnabled(OWUtilities.GetShipThrusterBank(__instance._thruster)))
		{
			__result = 0f;
			return false;
		}

		if (QSBPlayerManager.LocalPlayer.FlyingShip)
		{
			__result = Vector3.Dot(__instance._thrusterModel.GetLocalAcceleration(), __instance._thrusterFilter);
		}
		else
		{
			__result = Vector3.Dot(ShipTransformSync.LocalInstance.ThrusterVariableSyncer.AccelerationSyncer.Value, __instance._thrusterFilter);
		}
		
		return false;
	}
}
