using System;
using HarmonyLib;
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
			else if (__instance._dynamicMeteorPool.Count == 0)
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
			if (meteorController != null)
			{
				var qsbMeteorLauncher = QSBWorldSync.GetWorldFromUnity<QSBMeteorLauncher>(__instance);

				var launchSpeed = qsbMeteorLauncher.LaunchSpeed;
				var linearVelocity = __instance._parentBody.GetPointVelocity(__instance.transform.position) + __instance.transform.TransformDirection(__instance._launchDirection) * launchSpeed;
				var angularVelocity = __instance.transform.forward * 2f;
				meteorController.Launch(null, __instance.transform.position, __instance.transform.rotation, linearVelocity, angularVelocity);
				if (__instance._audioSector.ContainsOccupant(DynamicOccupant.Player))
				{
					__instance._launchSource.pitch = Random.Range(0.4f, 0.6f);
					__instance._launchSource.PlayOneShot(AudioType.BH_MeteorLaunch);
				}
			}

			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeteorController), nameof(MeteorController.OnCollisionEnter))]
		public static bool OnCollisionEnter(MeteorController __instance,
			Collision collision)
			=> false;


		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Impact))]
		public static bool Impact(MeteorController __instance,
			GameObject hitObject, Vector3 impactPoint, Vector3 impactVel)
		{
			var qsbMeteor = QSBWorldSync.GetWorldFromUnity<QSBMeteor>(__instance);

			var componentInParent = hitObject != null ? hitObject.GetComponentInParent<FragmentIntegrity>() : null;
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
			if (hitObject != null)
			{
				__instance.transform.SetParent(hitObject.GetAttachedOWRigidbody().transform);
			}
			FragmentSurfaceProxy.UntrackMeteor(__instance);
			FragmentCollisionProxy.UntrackMeteor(__instance);
			__instance._ignoringCollisions = false;
			__instance._hasImpacted = true;
			__instance._impactTime = Time.time;

			if (hitObject != null)
			{
				DebugLog.DebugWrite($"{qsbMeteor.LogName} - impact! {hitObject.name} {impactPoint} {impactVel} {damage}");
			}
			else
			{
				DebugLog.ToConsole($"{qsbMeteor.LogName} - got impact from server, but found no hit object locally "
					+ $"({impactPoint} {impactVel} {damage})", MessageType.Error);
			}

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
