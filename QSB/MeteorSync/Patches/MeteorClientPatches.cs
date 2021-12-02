using HarmonyLib;
using OWML.Common;
using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.MeteorSync.Patches
{
	public class MeteorClientPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.FixedUpdate))]
		public static bool FixedUpdate(MeteorLauncher __instance)
			=> false;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.LaunchMeteor))]
		public static bool LaunchMeteor(MeteorLauncher __instance)
		{
			var qsbMeteorLauncher = QSBWorldSync.GetWorldFromUnity<QSBMeteorLauncher>(__instance);

			MeteorController meteorController = null;
			QSBMeteor qsbMeteor;

			bool MeteorMatches(MeteorController x)
			{
				qsbMeteor = QSBWorldSync.GetWorldFromUnity<QSBMeteor>(x);
				return qsbMeteor.ObjectId == qsbMeteorLauncher.MeteorId;
			}

			if (__instance._meteorPool != null)
			{
				meteorController = __instance._meteorPool.Find(MeteorMatches);
				if (meteorController != null)
				{
					meteorController.Initialize(__instance.transform, __instance._detectableField, __instance._detectableFluid);
				}
			}
			else if (__instance._dynamicMeteorPool != null)
			{
				meteorController = __instance._dynamicMeteorPool.Find(MeteorMatches);
				if (meteorController != null)
				{
					meteorController.Initialize(__instance.transform, null, null);
				}
			}

			if (meteorController != null)
			{
				var linearVelocity = __instance._parentBody.GetPointVelocity(__instance.transform.position) + (__instance.transform.TransformDirection(__instance._launchDirection) * qsbMeteorLauncher.LaunchSpeed);
				var angularVelocity = __instance.transform.forward * 2f;
				meteorController.Launch(null, __instance.transform.position, __instance.transform.rotation, linearVelocity, angularVelocity);
				if (__instance._audioSector.ContainsOccupant(DynamicOccupant.Player))
				{
					__instance._launchSource.pitch = Random.Range(0.4f, 0.6f);
					__instance._launchSource.PlayOneShot(AudioType.BH_MeteorLaunch);
				}
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

			var qsbMeteor = QSBWorldSync.GetWorldFromUnity<QSBMeteor>(__instance);
			if (QSBMeteor.IsSpecialImpact(hitObject))
			{
				QSBEventManager.FireEvent(EventNames.QSBMeteorSpecialImpact, qsbMeteor);
			}

			return false;
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
			if (!QSBCore.WorldObjectsReady)
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
