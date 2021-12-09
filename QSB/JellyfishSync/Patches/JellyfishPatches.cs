using HarmonyLib;
using QSB.AuthoritySync;
using QSB.JellyfishSync.Events;
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
		[HarmonyPatch(typeof(JellyfishController), nameof(JellyfishController.OnSectorOccupantsUpdated))]
		public static bool OnSectorOccupantsUpdated(JellyfishController __instance)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return true;
			}
			var qsbJellyfish = __instance.GetWorldObject<QSBJellyfish>();

			if (!__instance.gameObject.activeSelf && __instance._sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship))
			{
				__instance.gameObject.SetActive(true);
				__instance._jellyfishBody.Unsuspend();
				qsbJellyfish.TransformSync.NetIdentity.FireAuthQueue(true);
				return false;
			}
			if (__instance.gameObject.activeSelf && !__instance._sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship))
			{
				__instance._jellyfishBody.Suspend();
				__instance.gameObject.SetActive(false);
				qsbJellyfish.TransformSync.NetIdentity.FireAuthQueue(false);
			}

			return false;
		}

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
					qsbJellyfish.SendMessage(new JellyfishRisingMessage(qsbJellyfish));
				}
			}
			else
			{
				__instance._jellyfishBody.AddAcceleration(-__instance.transform.up * __instance._downwardsAcceleration);
				if (sqrMagnitude < __instance._lowerLimit * __instance._lowerLimit)
				{
					qsbJellyfish.IsRising = true;
					qsbJellyfish.SendMessage(new JellyfishRisingMessage(qsbJellyfish));
				}
			}

			return false;
		}
	}
}
