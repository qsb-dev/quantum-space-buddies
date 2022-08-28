using HarmonyLib;
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
		if (!ModelShipThrusterVariableSyncer.LocalInstance.ThrusterFlameControllers.Contains(__instance))
		{
			return true;
		}

		if (!__instance._thrusterModel.IsThrusterBankEnabled(OWUtilities.GetShipThrusterBank(__instance._thruster)))
		{
			__result = 0f;
			return false;
		}

		if (QSBPlayerManager.LocalPlayer.FlyingModelShip)
		{
			__result = Vector3.Dot(__instance._thrusterModel.GetLocalAcceleration(), __instance._thrusterFilter);
		}
		else
		{
			__result = Vector3.Dot(ModelShipThrusterVariableSyncer.LocalInstance.AccelerationSyncer.Value, __instance._thrusterFilter);
		}

		return false;
	}
}
