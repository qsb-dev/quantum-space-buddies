using System;
using HarmonyLib;
using QSB.Events;
using QSB.MeteorSync.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;
using Random = UnityEngine.Random;

namespace QSB.MeteorSync.Patches
{
	public class MeteorServerPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnServerClientConnect;


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
			if (__instance._initialized && Time.time > __instance._lastLaunchTime + __instance._launchDelay)
			{
				if (!__instance._areParticlesPlaying)
				{
					__instance._areParticlesPlaying = true;
					foreach (var particleSystem in __instance._launchParticles)
					{
						particleSystem.Play();
					}

					var qsbMeteorLauncher = QSBWorldSync.GetWorldFromUnity<QSBMeteorLauncher>(__instance);
					QSBEventManager.FireEvent(EventNames.QSBMeteorPreLaunch, qsbMeteorLauncher);
					DebugLog.DebugWrite($"{qsbMeteorLauncher.LogName} - prelaunch");
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
				}
			}

			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.LaunchMeteor))]
		public static bool LaunchMeteor(MeteorLauncher __instance)
		{
			var flag = __instance._dynamicMeteorPool != null && (__instance._meteorPool == null || Random.value < __instance._dynamicProbability);
			MeteorController meteorController = null;
			if (!flag)
			{
				if (__instance._meteorPool.Count == 0)
				{
					Debug.LogWarning("MeteorLauncher is out of Meteors!", __instance);
				}
				else
				{
					meteorController = __instance._meteorPool[__instance._meteorPool.Count - 1];
					meteorController.Initialize(__instance.transform, __instance._detectableField, __instance._detectableFluid);
					__instance._meteorPool.QuickRemoveAt(__instance._meteorPool.Count - 1);
					__instance._launchedMeteors.Add(meteorController);
				}
			}
			else
			{
				if (__instance._dynamicMeteorPool.Count == 0)
				{
					Debug.LogWarning("MeteorLauncher is out of Dynamic Meteors!", __instance);
				}
				else
				{
					meteorController = __instance._dynamicMeteorPool[__instance._dynamicMeteorPool.Count - 1];
					meteorController.Initialize(__instance.transform, null, null);
					__instance._dynamicMeteorPool.QuickRemoveAt(__instance._dynamicMeteorPool.Count - 1);
					__instance._launchedDynamicMeteors.Add(meteorController);
				}
			}
			if (meteorController != null)
			{
				var qsbMeteorLauncher = QSBWorldSync.GetWorldFromUnity<QSBMeteorLauncher>(__instance);
				var qsbMeteor = QSBWorldSync.GetWorldFromUnity<QSBMeteor>(meteorController);

				qsbMeteorLauncher.MeteorId = qsbMeteor.ObjectId;
				qsbMeteorLauncher.LaunchSpeed = Random.Range(__instance._minLaunchSpeed, __instance._maxLaunchSpeed);

				var linearVelocity = __instance._parentBody.GetPointVelocity(__instance.transform.position) + __instance.transform.TransformDirection(__instance._launchDirection) * qsbMeteorLauncher.LaunchSpeed;
				var angularVelocity = __instance.transform.forward * 2f;
				meteorController.Launch(null, __instance.transform.position, __instance.transform.rotation, linearVelocity, angularVelocity);
				if (__instance._audioSector.ContainsOccupant(DynamicOccupant.Player))
				{
					__instance._launchSource.pitch = Random.Range(0.4f, 0.6f);
					__instance._launchSource.PlayOneShot(AudioType.BH_MeteorLaunch);
				}

				QSBEventManager.FireEvent(EventNames.QSBMeteorLaunch, qsbMeteorLauncher);
				DebugLog.DebugWrite($"{qsbMeteorLauncher.LogName} - launch {qsbMeteor.LogName} {qsbMeteorLauncher.LaunchSpeed}");
			}

			return false;
		}


		[HarmonyPostfix]
		[HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Impact))]
		public static void Impact(MeteorController __instance,
			GameObject hitObject, Vector3 impactPoint, Vector3 impactVel)
		{
			var qsbMeteor = QSBWorldSync.GetWorldFromUnity<QSBMeteor>(__instance);
			if (QSBMeteor.IsSpecialImpact(hitObject))
			{
				QSBEventManager.FireEvent(EventNames.QSBMeteorSpecialImpact, qsbMeteor);
				DebugLog.DebugWrite($"{qsbMeteor.LogName} - special impact {hitObject.name}");
			}
			else
			{
				DebugLog.DebugWrite($"{qsbMeteor.LogName} - impact {hitObject.name} {impactPoint} {impactVel}");
			}
		}


		[HarmonyPostfix]
		[HarmonyPatch(typeof(FragmentIntegrity), nameof(FragmentIntegrity.AddDamage))]
		public static void AddDamage(FragmentIntegrity __instance,
			float damage)
		{
			var qsbFragment = QSBWorldSync.GetWorldFromUnity<QSBFragment>(__instance);
			QSBEventManager.FireEvent(EventNames.QSBFragmentDamage, qsbFragment, damage);
			DebugLog.DebugWrite($"{qsbFragment.LogName} - damage {damage} {__instance._integrity} / {__instance._origIntegrity}");
		}
	}
}
