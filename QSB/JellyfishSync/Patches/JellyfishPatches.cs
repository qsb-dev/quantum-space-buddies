using HarmonyLib;
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
			if (!QSBCore.WorldObjectsReady)
			{
				return false;
			}
			var qsbJellyfish = QSBWorldSync.GetWorldFromUnity<QSBJellyfish>(__instance);

			if (!__instance.gameObject.activeSelf && __instance._sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship))
			{
				__instance.gameObject.SetActive(true);
				__instance._jellyfishBody.Unsuspend();
				QSBEventManager.FireEvent(EventNames.QSBSuspendChange, qsbJellyfish.TransformSync.NetIdentity, false);
				return false;
			}
			if (__instance.gameObject.activeSelf && !__instance._sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship))
			{
				__instance._jellyfishBody.Suspend();
				__instance.gameObject.SetActive(false);
				QSBEventManager.FireEvent(EventNames.QSBSuspendChange, qsbJellyfish.TransformSync.NetIdentity, true);
			}

			return false;
		}
	}
}
