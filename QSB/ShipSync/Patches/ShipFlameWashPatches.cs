using HarmonyLib;
using QSB.ModelShip;
using QSB.ModelShip.TransformSync;
using QSB.Patches;
using QSB.Player;
using QSB.ShipSync.TransformSync;
using UnityEngine;

namespace QSB.ShipSync.Patches;

internal class ShipFlameWashPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ThrusterFlameController), nameof(ThrusterFlameController.GetThrustFraction))]
	public static bool GetThrustFraction(ThrusterFlameController __instance, ref float __result)
	{
		if (!ShipThrusterManager.ShipFlameControllers.Contains(__instance))
		{
			return true;
		}

		if (!__instance._thrusterModel.IsThrusterBankEnabled(OWUtilities.GetShipThrusterBank(__instance._thruster)))
		{
			__result = 0f;
			return false;
		}

		if (QSBPlayerManager.LocalPlayer.FlyingShip)
		{
			__result = Vector3.Dot(__instance._thrusterModel.GetLocalAcceleration(), __instance._thrusterFilter);
		}
		else
		{
			__result = Vector3.Dot(ShipTransformSync.LocalInstance.ThrusterVariableSyncer.AccelerationSyncer.Value, __instance._thrusterFilter);
		}
		
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ThrusterWashController), nameof(ThrusterWashController.Update))]
	public static bool Update(ThrusterWashController __instance)
	{
		var isShip = ShipThrusterManager.ShipWashController == __instance;
		var isModelShip = ModelShipTransformSync.LocalInstance.ThrusterVariableSyncer.ThrusterWashController == __instance;

		if (!isShip && !isModelShip)
		{
			return true;
		}

		bool isLocal;
		Vector3 remoteAcceleration;
		if (isShip)
		{
			isLocal = QSBPlayerManager.LocalPlayer.FlyingShip;
			remoteAcceleration = ShipTransformSync.LocalInstance.ThrusterVariableSyncer.AccelerationSyncer.Value;
		}
		else
		{
			isLocal = QSBPlayerManager.LocalPlayer.FlyingModelShip;
			remoteAcceleration = ModelShipTransformSync.LocalInstance.ThrusterVariableSyncer.AccelerationSyncer.Value;
		}

		var localAcceleration = isLocal
			? __instance._thrusterModel.GetLocalAcceleration()
			: remoteAcceleration;

		// The rest of this is just copy pasted from the original method
		var hitInfo = default(RaycastHit);
		var aboveGround = false;
		var emissionScale = __instance._emissionThrusterScale.Evaluate(localAcceleration.y);
		if (emissionScale > 0f)
		{
			aboveGround = Physics.Raycast(__instance.transform.position, __instance.transform.forward, out hitInfo, __instance._raycastDistance, OWLayerMask.physicalMask);
		}

		emissionScale = aboveGround ? (emissionScale * __instance._emissionDistanceScale.Evaluate(hitInfo.distance)) : 0f;
		if (emissionScale > 0f)
		{
			var position = hitInfo.point + (hitInfo.normal * 0.25f);
			var rotation = Quaternion.LookRotation(hitInfo.normal);
			if (!__instance._defaultParticleSystem.isPlaying)
			{
				__instance._defaultParticleSystem.Play();
			}

			__instance._defaultEmissionModule.rateOverTimeMultiplier = __instance._baseDefaultEmissionRate * emissionScale;
			__instance._defaultParticleSystem.transform.SetPositionAndRotation(position, rotation);
			if (__instance._defaultMainModule.customSimulationSpace != hitInfo.transform)
			{
				__instance._defaultMainModule.customSimulationSpace = hitInfo.transform;
				__instance._defaultParticleSystem.Clear();
			}

			var hitSurfaceType = Locator.GetSurfaceManager().GetHitSurfaceType(hitInfo);
			var particleSystem = __instance._particleSystemBySurfaceType[(int)hitSurfaceType];
			if (particleSystem != __instance._activeSurfaceParticleSystem)
			{
				if (__instance._activeSurfaceParticleSystem != null)
				{
					__instance._activeSurfaceParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
				}

				if (particleSystem != null)
				{
					particleSystem.Play();
				}

				__instance._activeSurfaceParticleSystem = particleSystem;
			}

			if (__instance._activeSurfaceParticleSystem != null)
			{
				var es = __instance._activeSurfaceParticleSystem.emission;
				es.rateOverTimeMultiplier = __instance._baseSurfaceEmissionRate[(int)hitSurfaceType] * emissionScale;
				__instance._activeSurfaceParticleSystem.transform.position = hitInfo.point + (hitInfo.normal * 0.25f);
				__instance._activeSurfaceParticleSystem.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
				var main = __instance._activeSurfaceParticleSystem.main;
				if (main.customSimulationSpace != hitInfo.transform)
				{
					main.customSimulationSpace = hitInfo.transform;
					__instance._activeSurfaceParticleSystem.Clear();
					return false;
				}
			}
		}
		else
		{
			if (__instance._defaultParticleSystem.isPlaying)
			{
				__instance._defaultParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
			}

			if (__instance._activeSurfaceParticleSystem != null)
			{
				__instance._activeSurfaceParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
				__instance._activeSurfaceParticleSystem = null;
			}
		}

		return false;
	}
}
