using HarmonyLib;
using QSB.Patches;
using QSB.Player;
using UnityEngine;

namespace QSB.ShipSync.Patches;

internal class FlameWashPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ThrusterFlameController), nameof(ThrusterFlameController.GetThrustFraction))]
	public static bool FlameThrustFraction(ThrusterFlameController __instance, ref float __result)
	{
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
			__instance.OnStartTranslationalThrust();
			__result = Vector3.Dot(ShipManager.Instance.ShipThrusterSync.AccelerationVariableSyncer.Value, __instance._thrusterFilter);
		}
		
		return false;
	}
}