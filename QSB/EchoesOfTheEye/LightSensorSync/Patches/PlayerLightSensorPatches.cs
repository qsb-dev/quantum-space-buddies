using HarmonyLib;
using QSB.EchoesOfTheEye.LightSensorSync.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Tools.FlashlightTool;
using QSB.Tools.ProbeTool;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * For those who come here,
 * leave while you still can.
 */

namespace QSB.EchoesOfTheEye.LightSensorSync.Patches;

/// <summary>
/// remote player light sensors are disabled, so these will only run for the local player light sensor
/// </summary>
[HarmonyPatch(typeof(SingleLightSensor))]
internal class PlayerLightSensorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	/// <summary>
	/// to prevent allocating a new list every frame
	/// </summary>
	private static readonly List<DreamLanternController> _illuminatingDreamLanternList = new();

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SingleLightSensor.ManagedFixedUpdate))]
	private static bool ManagedFixedUpdate(SingleLightSensor __instance)
	{
		if (!LightSensorManager.IsPlayerLightSensor(__instance))
		{
			return true;
		}

		if (__instance._fixedUpdateFrameDelayCount > 0)
		{
			__instance._fixedUpdateFrameDelayCount--;
			return false;
		}
		var illuminated = __instance._illuminated;
		if (__instance._illuminatingDreamLanternList != null)
		{
			_illuminatingDreamLanternList.Clear();
			_illuminatingDreamLanternList.AddRange(__instance._illuminatingDreamLanternList);
		}
		__instance.UpdateIllumination();
		if (!illuminated && __instance._illuminated)
		{
			__instance.OnDetectLight.Invoke();
			new PlayerSetIlluminatedMessage(QSBPlayerManager.LocalPlayerId, true).Send();
		}
		else if (illuminated && !__instance._illuminated)
		{
			__instance.OnDetectDarkness.Invoke();
			new PlayerSetIlluminatedMessage(QSBPlayerManager.LocalPlayerId, false).Send();
		}
		if (__instance._illuminatingDreamLanternList != null &&
			!__instance._illuminatingDreamLanternList.SequenceEqual(_illuminatingDreamLanternList))
		{
			new PlayerIlluminatingLanternsMessage(QSBPlayerManager.LocalPlayerId, __instance._illuminatingDreamLanternList).Send();
		}
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SingleLightSensor.UpdateIllumination))]
	private static bool UpdateIllumination(SingleLightSensor __instance)
	{
		if (!LightSensorManager.IsPlayerLightSensor(__instance))
		{
			return true;
		}

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
							if (lightSource is QSBFlashlight qsbFlashlight)
							{
								var position = qsbFlashlight.Player.Camera.transform.position;
								var vector3 = __instance.transform.position - position;
								if (Vector3.Angle(qsbFlashlight.Player.Camera.transform.forward, vector3) <= __instance._maxSpotHalfAngle &&
									!__instance.CheckOcclusion(position, sensorWorldPos, sensorWorldDir))
								{
									__instance._illuminated = true;
								}
							}
							else
							{
								var position = Locator.GetPlayerCamera().transform.position;
								var vector3 = __instance.transform.position - position;
								if (Vector3.Angle(Locator.GetPlayerCamera().transform.forward, vector3) <= __instance._maxSpotHalfAngle &&
									!__instance.CheckOcclusion(position, sensorWorldPos, sensorWorldDir))
								{
									__instance._illuminated = true;
								}
							}
							break;
						}
					case LightSourceType.PROBE:
						{
							if (lightSource is QSBProbe qsbProbe)
							{
								var probe = qsbProbe;
								if (probe != null &&
									probe.IsLaunched() &&
									!probe.IsRetrieving() &&
									probe.CheckIlluminationAtPoint(sensorWorldPos, __instance._sensorRadius, __instance._maxDistance) &&
									!__instance.CheckOcclusion(probe.GetLightSourcePosition(), sensorWorldPos, sensorWorldDir))
								{
									__instance._illuminated = true;
								}
							}
							else
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
