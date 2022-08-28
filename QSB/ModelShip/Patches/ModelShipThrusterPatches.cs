using HarmonyLib;
using QSB.ModelShip.TransformSync;
using QSB.Patches;
using QSB.Player;
using System.Linq;
using UnityEngine;

namespace QSB.ModelShip.Patches;

internal class ModelShipThrusterPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ThrusterFlameController), nameof(ThrusterFlameController.GetThrustFraction))]
	public static bool GetThrustFraction(ThrusterFlameController __instance, ref float __result)
	{
		var modelShipThrusters = ModelShipTransformSync.LocalInstance?.ThrusterVariableSyncer;

		if (modelShipThrusters == null) return true;

		if (modelShipThrusters.ThrusterFlameControllers.Contains(__instance) && !QSBPlayerManager.LocalPlayer.FlyingModelShip)
		{
			if(__instance._thrusterModel.IsThrusterBankEnabled(OWUtilities.GetShipThrusterBank(__instance._thruster)))
			{
				__result = Vector3.Dot(modelShipThrusters.AccelerationSyncer.Value, __instance._thrusterFilter);
				return false;
			}
		}
		return true;
	}
}
