using HarmonyLib;
using QSB.Patches;

namespace QSB.MeteorSync.Patches
{
	public class MeteorClientPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.FixedUpdate))]
		public static bool FixedUpdate(MeteorLauncher __instance)
		{
			if (__instance._launchedMeteors != null)
			{
				for (var i = __instance._launchedMeteors.Count - 1; i >= 0; i--)
				{
					if (__instance._launchedMeteors[i] == null)
					{
						__instance._launchedMeteors.QuickRemoveAt(i);
					}
					else if (__instance._launchedMeteors[i].isSuspended)
					{
						__instance._meteorPool.Add(__instance._launchedMeteors[i]);
						__instance._launchedMeteors.QuickRemoveAt(i);
					}
				}
			}
			if (__instance._launchedDynamicMeteors != null)
			{
				for (var j = __instance._launchedDynamicMeteors.Count - 1; j >= 0; j--)
				{
					if (__instance._launchedDynamicMeteors[j] == null)
					{
						__instance._launchedDynamicMeteors.QuickRemoveAt(j);
					}
					else if (__instance._launchedDynamicMeteors[j].isSuspended)
					{
						__instance._dynamicMeteorPool.Add(__instance._launchedDynamicMeteors[j]);
						__instance._launchedDynamicMeteors.QuickRemoveAt(j);
					}
				}
			}

			return false;
		}


		[HarmonyPostfix]
		[HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Launch))]
		public static void Launch(MeteorController __instance)
		{
			foreach (var owCollider in __instance._owColliders)
			{
				owCollider.SetActivation(false);
			}
			__instance._owRigidbody.MakeKinematic();
		}
	}
}
