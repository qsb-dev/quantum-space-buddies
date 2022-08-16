using HarmonyLib;
using QSB.EchoesOfTheEye.LightSensorSync.Messages;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * For those who come here,
 * leave while you still can.
 */

namespace QSB.EchoesOfTheEye.LightSensorSync.Patches;

[HarmonyPatch(typeof(SingleLightSensor))]
internal class LightSensorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SingleLightSensor.Start))]
	private static bool Start(SingleLightSensor __instance)
	{
		if (__instance._lightDetector != null)
		{
			__instance._lightSources = new List<ILightSource>();
			__instance._lightSourceMask = LightSourceType.VOLUME_ONLY;
			if (__instance._detectFlashlight)
			{
				__instance._lightSourceMask |= LightSourceType.FLASHLIGHT;
			}

			if (__instance._detectProbe)
			{
				__instance._lightSourceMask |= LightSourceType.PROBE;
			}

			if (__instance._detectDreamLanterns)
			{
				__instance._lightSourceMask |= LightSourceType.DREAM_LANTERN;
			}

			if (__instance._detectSimpleLanterns)
			{
				__instance._lightSourceMask |= LightSourceType.SIMPLE_LANTERN;
			}

			__instance._lightDetector.OnLightVolumeEnter += __instance.OnLightSourceEnter;
			__instance._lightDetector.OnLightVolumeExit += __instance.OnLightSourceExit;
		}
		else
		{
			Debug.LogError("LightSensor has no LightSourceDetector", __instance);
		}

		if (__instance._sector != null)
		{
			__instance.enabled = false;
			__instance._lightDetector.GetShape().enabled = false;
			// dont do _startIlluminated stuff here since its done in the worldobject
			// player light sensors dont have it either so its okay
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SingleLightSensor.OnSectorOccupantsUpdated))]
	private static bool OnSectorOccupantsUpdated(SingleLightSensor __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		// player light sensors dont have a sector, so no need to worry about them
		var qsbLightSensor = __instance.GetWorldObject<QSBLightSensor>();

		var containsAnyOccupants = __instance._sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (containsAnyOccupants && !__instance.enabled)
		{
			__instance.enabled = true;
			__instance._lightDetector.GetShape().enabled = true;
			if (__instance._preserveStateWhileDisabled)
			{
				__instance._fixedUpdateFrameDelayCount = 10;
			}
		}
		else if (!containsAnyOccupants && __instance.enabled)
		{
			__instance.enabled = false;
			__instance._lightDetector.GetShape().enabled = false;
			if (!__instance._preserveStateWhileDisabled)
			{
				if (qsbLightSensor._locallyIlluminated)
				{
					qsbLightSensor._locallyIlluminated = false;
					qsbLightSensor.OnDetectLocalDarkness?.Invoke();
					qsbLightSensor.SendMessage(new SetIlluminatedMessage(false));
				}
			}
		}

		return false;
	}

	/// <summary>
	/// to prevent allocating a new list every frame
	/// </summary>
	private static readonly List<DreamLanternController> _illuminatingDreamLanternList = new();

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SingleLightSensor.ManagedFixedUpdate))]
	private static bool ManagedFixedUpdate(SingleLightSensor __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var isPlayerLightSensor = LightSensorManager.IsPlayerLightSensor(__instance);
		var qsbPlayerLightSensor = isPlayerLightSensor ? __instance.GetComponent<QSBPlayerLightSensor>() : null;
		var qsbLightSensor = isPlayerLightSensor ? null : __instance.GetWorldObject<QSBLightSensor>();

		if (__instance._fixedUpdateFrameDelayCount > 0)
		{
			__instance._fixedUpdateFrameDelayCount--;
		}

		if (__instance._illuminatingDreamLanternList != null)
		{
			_illuminatingDreamLanternList.Clear();
			_illuminatingDreamLanternList.AddRange(__instance._illuminatingDreamLanternList);
		}

		var illuminated = __instance._illuminated;
		__instance.UpdateIllumination();
		bool locallyIlluminated;
		if (isPlayerLightSensor)
		{
			locallyIlluminated = qsbPlayerLightSensor._locallyIlluminated;
			qsbPlayerLightSensor._locallyIlluminated = __instance._illuminated;
		}
		else
		{
			locallyIlluminated = qsbLightSensor._locallyIlluminated;
			qsbLightSensor._locallyIlluminated = __instance._illuminated;
		}

		__instance._illuminated = illuminated;

		if (isPlayerLightSensor)
		{
			if (!locallyIlluminated && qsbPlayerLightSensor._locallyIlluminated)
			{
				qsbPlayerLightSensor._locallyIlluminated = true;
				new PlayerSetIlluminatedMessage(qsbPlayerLightSensor.PlayerId, true).Send();
			}
			else if (locallyIlluminated && !qsbPlayerLightSensor._locallyIlluminated)
			{
				qsbPlayerLightSensor._locallyIlluminated = false;
				new PlayerSetIlluminatedMessage(qsbPlayerLightSensor.PlayerId, false).Send();
			}
		}
		else
		{
			if (!locallyIlluminated && qsbLightSensor._locallyIlluminated)
			{
				qsbLightSensor._locallyIlluminated = true;
				qsbLightSensor.OnDetectLocalLight?.Invoke();
				qsbLightSensor.SendMessage(new SetIlluminatedMessage(true));
			}
			else if (locallyIlluminated && !qsbLightSensor._locallyIlluminated)
			{
				qsbLightSensor._locallyIlluminated = false;
				qsbLightSensor.OnDetectLocalDarkness?.Invoke();
				qsbLightSensor.SendMessage(new SetIlluminatedMessage(false));
			}
		}

		if (__instance._illuminatingDreamLanternList != null
			&& !__instance._illuminatingDreamLanternList.SequenceEqual(_illuminatingDreamLanternList))
		{
			if (isPlayerLightSensor)
			{
				new PlayerIlluminatingLanternsMessage(qsbPlayerLightSensor.PlayerId, __instance._illuminatingDreamLanternList).Send();
			}
			else
			{
				qsbLightSensor.SendMessage(new IlluminatingLanternsMessage(__instance._illuminatingDreamLanternList));
			}
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SingleLightSensor.UpdateIllumination))]
	private static bool UpdateIllumination(SingleLightSensor __instance)
	{
		__instance._illuminated = false;
		__instance._illuminatingDreamLanternList?.Clear();
		if (__instance._lightSources == null || __instance._lightSources.Count == 0)
		{
			return false;
		}
		var sensorWorldPos = __instance.transform.TransformPoint(__instance._localSensorOffset);
		var sensorWorldDir = Vector3.zero;
		if (__instance._directionalSensor)
		{
			sensorWorldDir = __instance.transform.TransformDirection(__instance._localDirection).normalized;
		}
		foreach (var lightSource in __instance._lightSources)
		{
			if ((__instance._lightSourceMask & lightSource.GetLightSourceType()) == lightSource.GetLightSourceType() &&
				lightSource.CheckIlluminationAtPoint(sensorWorldPos, __instance._sensorRadius, __instance._maxDistance))
			{
				switch (lightSource.GetLightSourceType())
				{
					case LightSourceType.UNDEFINED:
						{
							var owlight = lightSource as OWLight2;
							var occludableLight = owlight.GetLight().shadows != LightShadows.None &&
								owlight.GetLight().shadowStrength > 0.5f;
							if (owlight.CheckIlluminationAtPoint(sensorWorldPos, __instance._sensorRadius, __instance._maxDistance) &&
								!__instance.CheckOcclusion(owlight.transform.position, sensorWorldPos, sensorWorldDir, occludableLight))
							{
								__instance._illuminated = true;
							}
							break;
						}
					case LightSourceType.FLASHLIGHT:
						{
							var position = Locator.GetPlayerCamera().transform.position;
							var vector3 = __instance.transform.position - position;
							if (Vector3.Angle(Locator.GetPlayerCamera().transform.forward, vector3) <= __instance._maxSpotHalfAngle &&
								!__instance.CheckOcclusion(position, sensorWorldPos, sensorWorldDir))
							{
								__instance._illuminated = true;
							}
							break;
						}
					case LightSourceType.PROBE:
						{
							var probe = Locator.GetProbe();
							if (probe != null &&
								probe.IsLaunched() &&
								!probe.IsRetrieving() &&
								probe.CheckIlluminationAtPoint(sensorWorldPos, __instance._sensorRadius, __instance._maxDistance) &&
								!__instance.CheckOcclusion(probe.GetLightSourcePosition(), sensorWorldPos, sensorWorldDir))
							{
								__instance._illuminated = true;
							}
							break;
						}
					case LightSourceType.DREAM_LANTERN:
						{
							var dreamLanternController = lightSource as DreamLanternController;
							if (dreamLanternController.IsLit() &&
								dreamLanternController.IsFocused(__instance._lanternFocusThreshold) &&
								dreamLanternController.CheckIlluminationAtPoint(sensorWorldPos, __instance._sensorRadius, __instance._maxDistance) &&
								!__instance.CheckOcclusion(dreamLanternController.GetLightPosition(), sensorWorldPos, sensorWorldDir))
							{
								__instance._illuminatingDreamLanternList.Add(dreamLanternController);
								__instance._illuminated = true;
							}
							break;
						}
					case LightSourceType.SIMPLE_LANTERN:
						foreach (var owlight in lightSource.GetLights())
						{
							var occludableLight = owlight.GetLight().shadows != LightShadows.None &&
								owlight.GetLight().shadowStrength > 0.5f;
							var maxDistance = Mathf.Min(__instance._maxSimpleLanternDistance, __instance._maxDistance);
							if (owlight.CheckIlluminationAtPoint(sensorWorldPos, __instance._sensorRadius, maxDistance) &&
								!__instance.CheckOcclusion(owlight.transform.position, sensorWorldPos, sensorWorldDir, occludableLight))
							{
								__instance._illuminated = true;
								break;
							}
						}
						break;
					case LightSourceType.VOLUME_ONLY:
						__instance._illuminated = true;
						break;
				}
			}
		}
		return false;
	}
}
