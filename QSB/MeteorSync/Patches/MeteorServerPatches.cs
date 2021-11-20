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

				var linearVelocity = __instance._parentBody.GetPointVelocity(__instance.transform.position) + (__instance.transform.TransformDirection(__instance._launchDirection) * qsbMeteorLauncher.LaunchSpeed);
				var angularVelocity = __instance.transform.forward * 2f;
				meteorController.Launch(null, __instance.transform.position, __instance.transform.rotation, linearVelocity, angularVelocity);
				if (__instance._audioSector.ContainsOccupant(DynamicOccupant.Player))
				{
					__instance._launchSource.pitch = Random.Range(0.4f, 0.6f);
					__instance._launchSource.PlayOneShot(AudioType.BH_MeteorLaunch);
				}

				QSBEventManager.FireEvent(EventNames.QSBMeteorLaunch, qsbMeteorLauncher);
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
			}
		}


		[HarmonyPostfix]
		[HarmonyPatch(typeof(FragmentIntegrity), nameof(FragmentIntegrity.AddDamage))]
		public static void AddDamage(FragmentIntegrity __instance,
			float damage)
		{
			var qsbFragment = QSBWorldSync.GetWorldFromUnity<QSBFragment>(__instance);
			QSBEventManager.FireEvent(EventNames.QSBFragmentDamage, qsbFragment, damage);
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(DetachableFragment), nameof(DetachableFragment.Detach))]
		public static void Detach_Prefix(DetachableFragment __instance, out FragmentIntegrity __state) =>
			// this gets set to null in Detach, so store it here and and then restore it in postfix
			__state = __instance._fragmentIntegrity;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(DetachableFragment), nameof(DetachableFragment.Detach))]
		public static void Detach_Postfix(DetachableFragment __instance, FragmentIntegrity __state) =>
			__instance._fragmentIntegrity = __state;


		[HarmonyPrefix]
		[HarmonyPatch(typeof(DebrisLeash), nameof(DebrisLeash.MoveByDistance))]
		public static bool MoveByDistance(DebrisLeash __instance,
			float distance)
		{
			if (__instance._detachableFragment == null || __instance._detachableFragment._fragmentIntegrity == null)
			{
				return true;
			}
			var qsbFragment = QSBWorldSync.GetWorldFromUnity<QSBFragment>(__instance._detachableFragment._fragmentIntegrity);

			if (__instance.enabled)
			{
				var vector = __instance._attachedBody.GetPosition() - __instance._anchorBody.GetPosition();
				var d = Mathf.Min(distance, qsbFragment.LeashLength - vector.magnitude);
				__instance._attachedBody.SetPosition(__instance._anchorBody.GetPosition() + (vector.normalized * d));
			}

			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(DebrisLeash), nameof(DebrisLeash.FixedUpdate))]
		public static bool FixedUpdate(DebrisLeash __instance)
		{
			if (__instance._detachableFragment == null || __instance._detachableFragment._fragmentIntegrity == null)
			{
				return true;
			}
			var qsbFragment = QSBWorldSync.GetWorldFromUnity<QSBFragment>(__instance._detachableFragment._fragmentIntegrity);

			if (!__instance._deccelerating)
			{
				var num = Vector3.Distance(__instance._attachedBody.GetPosition(), __instance._anchorBody.GetPosition());
				var num2 = Mathf.Pow(__instance._attachedBody.GetVelocity().magnitude, 2f) / (2f * __instance._deccel);
				var vector = __instance._attachedBody.GetVelocity() - __instance._anchorBody.GetVelocity();
				if (num >= qsbFragment.LeashLength - num2 && vector.magnitude > 0.1f)
				{
					__instance._deccelerating = true;
					return false;
				}
			}
			else
			{
				var vector2 = __instance._attachedBody.GetVelocity() - __instance._anchorBody.GetVelocity();
				var velocityChange = -vector2.normalized * Mathf.Min(__instance._deccel * Time.deltaTime, vector2.magnitude);
				if (velocityChange.magnitude < 0.01f)
				{
					__instance._attachedBody.SetVelocity(__instance._anchorBody.GetVelocity());
					__instance._deccelerating = false;
					if (__instance._detachableFragment != null)
					{
						__instance._detachableFragment.ComeToRest(__instance._anchorBody);
					}
					__instance.enabled = false;
					return false;
				}
				__instance._attachedBody.AddVelocityChange(velocityChange);
			}

			return false;
		}
	}
}
