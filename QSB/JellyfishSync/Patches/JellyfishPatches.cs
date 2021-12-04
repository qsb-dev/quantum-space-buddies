using HarmonyLib;
using QSB.AuthoritySync;
using QSB.Events;
using QSB.JellyfishSync.WorldObjects;
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
			var qsbJellyfish = QSBWorldSync.GetWorldFromUnity<QSBJellyfish>(__instance);

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
			var qsbJellyfish = QSBWorldSync.GetWorldFromUnity<QSBJellyfish>(__instance);

			var sqrMagnitude = (__instance._jellyfishBody.GetPosition() - __instance._planetBody.GetPosition()).sqrMagnitude;
			if (qsbJellyfish.IsRising)
			{
				__instance._jellyfishBody.AddAcceleration(__instance.transform.up * __instance._upwardsAcceleration);
				if (sqrMagnitude > __instance._upperLimit * __instance._upperLimit)
				{
					qsbJellyfish.IsRising = false;
					QSBEventManager.FireEvent(EventNames.QSBJellyfishRising, qsbJellyfish);
				}
			}
			else
			{
				__instance._jellyfishBody.AddAcceleration(-__instance.transform.up * __instance._downwardsAcceleration);
				if (sqrMagnitude < __instance._lowerLimit * __instance._lowerLimit)
				{
					qsbJellyfish.IsRising = true;
					QSBEventManager.FireEvent(EventNames.QSBJellyfishRising, qsbJellyfish);
				}
			}

			return false;
		}
	}
}
