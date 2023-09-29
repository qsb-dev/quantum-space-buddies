using HarmonyLib;
using QSB.ModelShip.TransformSync;
using QSB.Patches;
using QSB.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.ModelShip.Patches;

public class ModelShipThrusterAudioPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ThrusterModel), nameof(ThrusterModel.GetThrustFraction))]
	public static bool ThrusterModel_GetThrustFraction(ThrusterModel __instance, ref float __result)
	{
		if (__instance == ModelShipTransformSync.LocalInstance?.ThrusterVariableSyncer?.ThrusterModel && !QSBPlayerManager.LocalPlayer.FlyingModelShip)
		{
			__result = ModelShipTransformSync.LocalInstance.ThrusterVariableSyncer.AccelerationSyncer.Value.magnitude / __instance._maxTranslationalThrust;
			return false;
		}
		else
		{
			return true;
		}
	}
}
