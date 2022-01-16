using HarmonyLib;
using QSB.JellyfishSync.Messages;
using QSB.JellyfishSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.JellyfishSync.Patches
{
	public class JellyfishPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(JellyfishController), nameof(JellyfishController.FixedUpdate))]
		public static bool FixedUpdate(JellyfishController __instance)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return true;
			}

			var qsbJellyfish = __instance.GetWorldObject<QSBJellyfish>();

			var sqrMagnitude = (__instance._jellyfishBody.GetPosition() - __instance._planetBody.GetPosition()).sqrMagnitude;
			if (qsbJellyfish.IsRising)
			{
				__instance._jellyfishBody.AddAcceleration(__instance.transform.up * __instance._upwardsAcceleration);
				if (sqrMagnitude > __instance._upperLimit * __instance._upperLimit)
				{
					qsbJellyfish.IsRising = false;

					qsbJellyfish.SendMessage(new JellyfishRisingMessage(qsbJellyfish.IsRising));
				}
			}
			else
			{
				__instance._jellyfishBody.AddAcceleration(-__instance.transform.up * __instance._downwardsAcceleration);
				if (sqrMagnitude < __instance._lowerLimit * __instance._lowerLimit)
				{
					qsbJellyfish.IsRising = true;
					qsbJellyfish.SendMessage(new JellyfishRisingMessage(qsbJellyfish.IsRising));
				}
			}

			return false;
		}
	}
}
