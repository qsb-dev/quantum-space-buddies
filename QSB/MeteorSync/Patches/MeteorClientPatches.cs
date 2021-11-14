using HarmonyLib;
using OWML.Common;
using QSB.Events;
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
			=> false;


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
				var linearVelocity = __instance._parentBody.GetPointVelocity(__instance.transform.position) + __instance.transform.TransformDirection(__instance._launchDirection) * qsbMeteorLauncher.LaunchSpeed;
				var angularVelocity = __instance.transform.forward * 2f;
				meteorController.Launch(null, __instance.transform.position, __instance.transform.rotation, linearVelocity, angularVelocity);
				if (__instance._audioSector.ContainsOccupant(DynamicOccupant.Player))
				{
					__instance._launchSource.pitch = Random.Range(0.4f, 0.6f);
					__instance._launchSource.PlayOneShot(AudioType.BH_MeteorLaunch);
				}

				DebugLog.DebugWrite($"{qsbMeteorLauncher.LogName} - launch {qsbMeteor.LogName} {qsbMeteorLauncher.LaunchSpeed}");
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
				DebugLog.DebugWrite($"{qsbMeteor.LogName} - special impact {hitObject.name}");
			}
			else
			{
				DebugLog.DebugWrite($"{qsbMeteor.LogName} - impact {hitObject.name} {impactPoint} {impactVel}");
			}

			return false;
		}
	}
}
