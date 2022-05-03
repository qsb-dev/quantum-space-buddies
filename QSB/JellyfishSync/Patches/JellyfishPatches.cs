using HarmonyLib;
using QSB.JellyfishSync.Messages;
using QSB.JellyfishSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.JellyfishSync.Patches;

public class JellyfishPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(JellyfishController), nameof(JellyfishController.FixedUpdate))]
	public static bool FixedUpdate(JellyfishController __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var sqrMagnitude = (__instance._jellyfishBody.GetPosition() - __instance._planetBody.GetPosition()).sqrMagnitude;
		if (__instance._isRising)
		{
			__instance._jellyfishBody.AddAcceleration(__instance.transform.up * __instance._upwardsAcceleration);
			if (sqrMagnitude > __instance._upperLimit * __instance._upperLimit)
			{
				__instance._isRising = false;
				__instance._attractiveFluidVolume.SetVolumeActivation(true);
				__instance.GetWorldObject<QSBJellyfish>().SendMessage(new JellyfishRisingMessage(false));
				return false;
			}
		}
		else
		{
			__instance._jellyfishBody.AddAcceleration(-__instance.transform.up * __instance._downwardsAcceleration);
			if (sqrMagnitude < __instance._lowerLimit * __instance._lowerLimit)
			{
				__instance._isRising = true;
				__instance._attractiveFluidVolume.SetVolumeActivation(false);
				__instance.GetWorldObject<QSBJellyfish>().SendMessage(new JellyfishRisingMessage(true));
			}
		}

		return false;
	}
}