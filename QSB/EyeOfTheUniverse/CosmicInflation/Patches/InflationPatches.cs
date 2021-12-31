using HarmonyLib;
using QSB.EyeOfTheUniverse.CosmicInflation.Messages;
using QSB.Messaging;
using QSB.Patches;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.CosmicInflation.Patches
{
	internal class InflationPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CosmicInflationController), nameof(CosmicInflationController.OnEnterFogSphere))]
		public static bool OnEnterFogSphere(CosmicInflationController __instance, GameObject obj)
		{
			if (obj.CompareTag("PlayerCameraDetector") && __instance._state == CosmicInflationController.State.ReadyToCollapse)
			{
				__instance._smokeSphereTrigger.SetTriggerActivation(false);
				__instance._probeDestroyTrigger.SetTriggerActivation(false);
				__instance.StartCollapse();
				new EnterFogSphereMessage().Send();
			}

			return false;
		}
	}
}
