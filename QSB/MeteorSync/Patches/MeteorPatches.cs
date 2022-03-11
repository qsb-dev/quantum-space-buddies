using HarmonyLib;
using OWML.Common;
using QSB.Messaging;
using QSB.MeteorSync.Messages;
using QSB.MeteorSync.WorldObjects;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.MeteorSync.Patches;

/// <summary>
/// server only
/// </summary>
public class MeteorServerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnServerClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.FixedUpdate))]
	public static bool MeteorLauncher_FixedUpdate(MeteorLauncher __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

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
				foreach (var launchParticle in __instance._launchParticles)
				{
					launchParticle.Play();
				}

				__instance.GetWorldObject<QSBMeteorLauncher>()
					.SendMessage(new MeteorPreLaunchMessage());
			}

			if (Time.time > __instance._lastLaunchTime + __instance._launchDelay + 2.3f)
			{
				__instance.LaunchMeteor();
				__instance._lastLaunchTime = Time.time;
				__instance._launchDelay = Random.Range(__instance._minInterval, __instance._maxInterval);
				__instance._areParticlesPlaying = false;
				foreach (var launchParticle in __instance._launchParticles)
				{
					launchParticle.Stop();
				}
			}
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.LaunchMeteor))]
	public static bool MeteorLauncher_LaunchMeteor(MeteorLauncher __instance)
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
			var launchSpeed = Random.Range(__instance._minLaunchSpeed, __instance._maxLaunchSpeed);

			var linearVelocity = __instance._parentBody.GetPointVelocity(__instance.transform.position) + __instance.transform.TransformDirection(__instance._launchDirection) * launchSpeed;
			var angularVelocity = __instance.transform.forward * 2f;
			meteorController.Launch(null, __instance.transform.position, __instance.transform.rotation, linearVelocity, angularVelocity);
			if (__instance._audioSector.ContainsOccupant(DynamicOccupant.Player))
			{
				__instance._launchSource.pitch = Random.Range(0.4f, 0.6f);
				__instance._launchSource.PlayOneShot(AudioType.BH_MeteorLaunch);
			}

			__instance.GetWorldObject<QSBMeteorLauncher>()
				.SendMessage(new MeteorLaunchMessage(meteorController, launchSpeed));
		}

		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(FragmentIntegrity), nameof(FragmentIntegrity.AddDamage))]
	public static void FragmentIntegrity_AddDamage(FragmentIntegrity __instance) =>
		__instance.GetWorldObject<QSBFragment>()
			.SendMessage(new FragmentIntegrityMessage(__instance._integrity));
}

/// <summary>
/// client only
/// </summary>
public class MeteorClientPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.FixedUpdate))]
	public static bool MeteorLauncher_FixedUpdate()
		=> false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(FragmentIntegrity), nameof(FragmentIntegrity.AddDamage))]
	public static bool FragmentIntegrity_AddDamage()
		=> false;
}

/// <summary>
/// both server and client
/// </summary>
public class MeteorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Impact))]
	public static void MeteorController_Impact(MeteorController __instance,
		GameObject hitObject, Vector3 impactPoint, Vector3 impactVel)
	{
		if (QSBMeteor.IsSpecialImpact(hitObject))
		{
			__instance.GetWorldObject<QSBMeteor>()
				.SendMessage(new MeteorSpecialImpactMessage());
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DetachableFragment), nameof(DetachableFragment.Detach))]
	public static void DetachableFragment_Detach_Prefix(DetachableFragment __instance, out FragmentIntegrity __state) =>
		// this gets set to null in Detach, so store it here and and then restore it in postfix
		__state = __instance._fragmentIntegrity;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DetachableFragment), nameof(DetachableFragment.Detach))]
	public static void DetachableFragment_Detach_Postfix(DetachableFragment __instance, FragmentIntegrity __state) =>
		__instance._fragmentIntegrity = __state;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DebrisLeash), nameof(DebrisLeash.Init))]
	public static void DebrisLeash_Init(DebrisLeash __instance)
	{
		if (__instance._detachableFragment == null || __instance._detachableFragment._fragmentIntegrity == null)
		{
			return;
		}

		var qsbFragment = __instance._detachableFragment._fragmentIntegrity.GetWorldObject<QSBFragment>();
		if (qsbFragment.LeashLength != null)
		{
			__instance._leashLength = (float)qsbFragment.LeashLength;
		}
		else
		{
			DebugLog.ToConsole($"DebrisLeash.Init called for {qsbFragment} before LeashLength was set", MessageType.Warning);
		}
	}
}
