using System;
using HarmonyLib;
using OWML.Common;
using QSB.MeteorSync.WorldObjects;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;
using Random = UnityEngine.Random;

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

			// skip meteor launching

			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.LaunchMeteor))]
		public static bool LaunchMeteor(MeteorLauncher __instance)
		{
			var qsbMeteorLauncher = QSBWorldSync.GetWorldFromUnity<QSBMeteorLauncher>(__instance);

			MeteorController meteorController = null;
			QSBMeteor qsbMeteor = null;

			bool MeteorMatches(MeteorController x)
			{
				qsbMeteor = QSBWorldSync.GetWorldFromUnity<QSBMeteor>(x);
				return qsbMeteor.ObjectId == qsbMeteorLauncher.MeteorId;
			}

			if (__instance._meteorPool != null)
			{
				var poolIndex = __instance._meteorPool.FindIndex(MeteorMatches);
				if (poolIndex != -1)
				{
					meteorController = __instance._meteorPool[poolIndex];
					meteorController.Initialize(__instance.transform, __instance._detectableField, __instance._detectableFluid);
					__instance._meteorPool.QuickRemoveAt(poolIndex);
					__instance._launchedMeteors.Add(meteorController);
				}
			}
			else if (__instance._dynamicMeteorPool != null)
			{
				var poolIndex = __instance._dynamicMeteorPool.FindIndex(MeteorMatches);
				if (poolIndex != -1)
				{
					meteorController = __instance._dynamicMeteorPool[poolIndex];
					meteorController.Initialize(__instance.transform, null, null);
					__instance._dynamicMeteorPool.QuickRemoveAt(poolIndex);
					__instance._launchedDynamicMeteors.Add(meteorController);
				}
			}
			if (meteorController != null)
			{
				qsbMeteor.Damage = qsbMeteorLauncher.Damage;

				var linearVelocity = __instance._parentBody.GetPointVelocity(__instance.transform.position) + __instance.transform.TransformDirection(__instance._launchDirection) * qsbMeteorLauncher.LaunchSpeed;
				var angularVelocity = __instance.transform.forward * 2f;
				meteorController.Launch(null, __instance.transform.position, __instance.transform.rotation, linearVelocity, angularVelocity);
				if (__instance._audioSector.ContainsOccupant(DynamicOccupant.Player))
				{
					__instance._launchSource.pitch = Random.Range(0.4f, 0.6f);
					__instance._launchSource.PlayOneShot(AudioType.BH_MeteorLaunch);
				}

				DebugLog.DebugWrite($"{qsbMeteorLauncher.LogName} - launch {qsbMeteor.LogName} {qsbMeteorLauncher.LaunchSpeed} {qsbMeteor.Damage}");
			}
			else
			{
				DebugLog.ToConsole($"{qsbMeteorLauncher.LogName} - could not find meteor {qsbMeteorLauncher.MeteorId} in pool", MessageType.Warning);
			}

			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Impact))]
		public static bool Impact(MeteorController __instance,
			GameObject hitObject, Vector3 impactPoint, Vector3 impactVel)
		{
			var qsbMeteor = QSBWorldSync.GetWorldFromUnity<QSBMeteor>(__instance);

			var componentInParent = hitObject.GetComponentInParent<FragmentIntegrity>();
			var damage = qsbMeteor.Damage;
			if (componentInParent != null)
			{
				if (!componentInParent.GetIgnoreMeteorDamage())
				{
					componentInParent.AddDamage(damage);
				}
				else if (componentInParent.GetParentFragment() != null && !componentInParent.GetParentFragment().GetIgnoreMeteorDamage())
				{
					componentInParent.GetParentFragment().AddDamage(damage);
				}
			}
			MeteorImpactMapper.RecordImpact(impactPoint, componentInParent);
			__instance._intactRenderer.enabled = false;
			__instance._impactLight.enabled = true;
			__instance._impactLight.intensity = __instance._impactLightCurve.Evaluate(0f);
			var rotation = Quaternion.LookRotation(impactVel);
			foreach (var particleSystem in __instance._impactParticles)
			{
				particleSystem.transform.rotation = rotation;
				particleSystem.Play();
			}
			__instance._impactSource.PlayOneShot(AudioType.BH_MeteorImpact);
			foreach (var owCollider in __instance._owColliders)
			{
				owCollider.SetActivation(false);
			}
			__instance._owRigidbody.MakeKinematic();
			__instance.transform.SetParent(hitObject.GetAttachedOWRigidbody().transform);
			FragmentSurfaceProxy.UntrackMeteor(__instance);
			FragmentCollisionProxy.UntrackMeteor(__instance);
			__instance._ignoringCollisions = false;
			__instance._hasImpacted = true;
			__instance._impactTime = Time.time;

			DebugLog.DebugWrite($"{qsbMeteor.LogName} - impact! {hitObject.name} {impactPoint} {impactVel} {damage}");

			return false;
		}


		[HarmonyPostfix]
		[HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Suspend), new Type[0])]
		public static void Suspend(MeteorController __instance)
		{
			if (!MeteorManager.MeteorsReady)
			{
				return;
			}

			var qsbMeteor = QSBWorldSync.GetWorldFromUnity<QSBMeteor>(__instance);
			DebugLog.DebugWrite($"{qsbMeteor.LogName} - suspended");
		}
	}
}
