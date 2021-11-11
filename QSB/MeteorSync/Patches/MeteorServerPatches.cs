using HarmonyLib;
using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.MeteorSync.Patches
{
	public class MeteorServerPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnServerClientConnect;


		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.FixedUpdate))]
		public static bool LaunchMeteor(MeteorLauncher __instance)
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
			if (__instance._initialized && Time.time > __instance._lastLaunchTime + __instance._launchDelay)
			{
				var qsbMeteorLauncher = QSBWorldSync.GetWorldFromUnity<QSBMeteorLauncher>(__instance);
				if (!__instance._areParticlesPlaying)
				{
					__instance._areParticlesPlaying = true;
					foreach (var particleSystem in __instance._launchParticles)
					{
						particleSystem.Play();
					}

					QSBEventManager.FireEvent(EventNames.QSBMeteorLaunch, qsbMeteorLauncher.ObjectId, true);
				}
				if (Time.time > __instance._lastLaunchTime + __instance._launchDelay + 2.3f)
				{
					__instance.LaunchMeteor();
					__instance._lastLaunchTime = Time.time;
					__instance._launchDelay = Random.Range(__instance._minInterval, __instance._maxInterval);
					__instance._areParticlesPlaying = false;
					foreach (var particleSystem in __instance._launchParticles)
					{
						particleSystem.Stop();
					}

					QSBEventManager.FireEvent(EventNames.QSBMeteorLaunch, qsbMeteorLauncher.ObjectId, false);
				}
			}

			return false;
		}


		[HarmonyPostfix]
		[HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Impact))]
		public static void Impact(MeteorController __instance)
		{
			var qsbMeteor = QSBWorldSync.GetWorldFromUnity<QSBMeteor>(__instance);
			QSBEventManager.FireEvent(EventNames.QSBMeteorImpact, qsbMeteor);
		}
	}
}
