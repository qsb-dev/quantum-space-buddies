using HarmonyLib;
using QSB.Messaging;
using QSB.MeteorSync.Messages;
using QSB.MeteorSync.WorldObjects;
using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.MeteorSync.Patches
{
	/// <summary>
	/// server only
	/// </summary>
	public class MeteorServerPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnServerClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.FixedUpdate))]
		public static bool FixedUpdate(MeteorLauncher __instance)
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
					foreach (var particleSystem in __instance._launchParticles)
					{
						particleSystem.Play();
					}

					__instance.GetWorldObject<QSBMeteorLauncher>().SendMessage(new MeteorPreLaunchMessage());
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
			if (!QSBWorldSync.AllObjectsReady)
			{
				return true;
			}

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

				__instance.GetWorldObject<QSBMeteorLauncher>().SendMessage(new MeteorLaunchMessage(meteorController, launchSpeed));
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(FragmentIntegrity), nameof(FragmentIntegrity.AddDamage))]
		public static bool AddDamage(FragmentIntegrity __instance,
			float damage)
		{
			if (__instance._integrity <= 0f)
			{
				return false;
			}

			__instance._integrity = __instance.CanBreak() ?
				Mathf.Max(0f, __instance._integrity - damage * __instance.DamageMultiplier()) :
				Mathf.Max(0f, __instance._integrity - Mathf.Min(damage * __instance.DamageMultiplier(), __instance._integrity / 2f));

			if (__instance._integrity == 0f && __instance._motherFragment != null)
			{
				__instance._motherFragment.ChildIsBroken();
			}

			__instance.CallOnTakeDamage();
			__instance.GetWorldObject<QSBFragment>().SendMessage(new FragmentDamageMessage(damage));

			return false;
		}
	}

	/// <summary>
	/// client only
	/// </summary>
	public class MeteorClientPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.FixedUpdate))]
		public static bool FixedUpdate(MeteorLauncher __instance) => false;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(FragmentIntegrity), nameof(FragmentIntegrity.AddDamage))]
		public static bool AddDamage() => false;
	}

	/// <summary>
	/// both server and client
	/// </summary>
	public class MeteorPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Impact))]
		public static void Impact(MeteorController __instance,
			GameObject hitObject)
		{
			if (!QSBWorldSync.AllObjectsReady)
			{
				return;
			}

			if (QSBMeteor.IsSpecialImpact(hitObject))
			{
				__instance.GetWorldObject<QSBMeteor>().SendMessage(new MeteorSpecialImpactMessage());
			}
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

			if (!QSBWorldSync.AllObjectsReady)
			{
				return true;
			}

			var qsbFragment = __instance._detachableFragment._fragmentIntegrity.GetWorldObject<QSBFragment>();

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

			if (!QSBWorldSync.AllObjectsReady)
			{
				return true;
			}

			var qsbFragment = __instance._detachableFragment._fragmentIntegrity.GetWorldObject<QSBFragment>();

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
