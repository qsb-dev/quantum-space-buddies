using HarmonyLib;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Patches;
using QSB.Player;
using QSB.Tools.FlashlightTool;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.EchoesOfTheEye.LightSensorSync.Patches;

[HarmonyPatch(typeof(SingleLightSensor))]
internal class LightSensorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SingleLightSensor.UpdateIllumination))]
	private static bool UpdateIllumination(SingleLightSensor __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var qsbLightSensor = __instance.GetWorldObject<QSBLightSensor>();
		var illuminatedByLocal = qsbLightSensor.IlluminatedByLocal;
		qsbLightSensor.IlluminatedByLocal = false;

		__instance._illuminated = false;
		__instance._illuminatingDreamLanternList?.Clear();

		if (__instance._lightSources == null || __instance._lightSources.Count == 0)
		{
			if (illuminatedByLocal)
			{
				qsbLightSensor.OnDetectLocalDarkness?.Invoke();
			}

			return false;
		}

		var vector = __instance.transform.TransformPoint(__instance._localSensorOffset);
		var sensorWorldDir = Vector3.zero;
		if (__instance._directionalSensor)
		{
			sensorWorldDir = __instance.transform.TransformDirection(__instance._localDirection).normalized;
		}

		foreach (var lightSource in __instance._lightSources)
		{
			if ((__instance._lightSourceMask & lightSource.GetLightSourceType()) == lightSource.GetLightSourceType() &&
			    lightSource.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance))
			{
				var lightSourceType = lightSource.GetLightSourceType();
				switch (lightSourceType)
				{
					case LightSourceType.UNDEFINED:
						{
							var owlight = lightSource as OWLight2;
							var occludableLight = owlight.GetLight().shadows != LightShadows.None && owlight.GetLight().shadowStrength > 0.5f;
							if (owlight.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance)
							    && !__instance.CheckOcclusion(owlight.transform.position, vector, sensorWorldDir, occludableLight))
							{
								__instance._illuminated = true;
								qsbLightSensor.IlluminatedByLocal = true;
							}

							break;
						}
					case LightSourceType.FLASHLIGHT:
						{
							var player = lightSource as Flashlight == Locator.GetFlashlight()
								? QSBPlayerManager.LocalPlayer
								: QSBPlayerManager.PlayerList.First(x => lightSource as QSBFlashlight == x.FlashLight);
							var playerCamera = player.Camera;

							var position = playerCamera.transform.position;
							var to = __instance.transform.position - position;
							if (Vector3.Angle(playerCamera.transform.forward, to) <= __instance._maxSpotHalfAngle
							    && !__instance.CheckOcclusion(position, vector, sensorWorldDir))
							{
								__instance._illuminated = true;
								qsbLightSensor.IlluminatedByLocal = player == QSBPlayerManager.LocalPlayer;
							}

							break;
						}
					case LightSourceType.PROBE: // todo remote probes
						{
							var probe = Locator.GetProbe();
							if (probe != null
							    && probe.IsLaunched()
							    && !probe.IsRetrieving()
							    && probe.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance)
							    && !__instance.CheckOcclusion(probe.GetLightSourcePosition(), vector, sensorWorldDir))
							{
								__instance._illuminated = true;
								qsbLightSensor.IlluminatedByLocal = true;
							}

							break;
						}
					case LightSourceType.FLASHLIGHT | LightSourceType.PROBE:
					case LightSourceType.FLASHLIGHT | LightSourceType.DREAM_LANTERN:
					case LightSourceType.PROBE | LightSourceType.DREAM_LANTERN:
					case LightSourceType.FLASHLIGHT | LightSourceType.PROBE | LightSourceType.DREAM_LANTERN:
						break;
					case LightSourceType.DREAM_LANTERN:
						{
							var dreamLanternController = lightSource as DreamLanternController;
							if (dreamLanternController.IsLit()
							    && dreamLanternController.IsFocused(__instance._lanternFocusThreshold)
							    && dreamLanternController.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance)
							    && !__instance.CheckOcclusion(dreamLanternController.GetLightPosition(), vector, sensorWorldDir))
							{
								__instance._illuminatingDreamLanternList.Add(dreamLanternController);
								__instance._illuminated = true;
								qsbLightSensor.IlluminatedByLocal = true; // todo remote dream lanterns
							}

							break;
						}
					case LightSourceType.SIMPLE_LANTERN:
						foreach (var owlight in lightSource.GetLights())
						{
							var occludableLight = owlight.GetLight().shadows != LightShadows.None && owlight.GetLight().shadowStrength > 0.5f;
							var maxDistance = Mathf.Min(__instance._maxSimpleLanternDistance, __instance._maxDistance);
							if (owlight.CheckIlluminationAtPoint(vector, __instance._sensorRadius, maxDistance)
							    && !__instance.CheckOcclusion(owlight.transform.position, vector, sensorWorldDir, occludableLight))
							{
								__instance._illuminated = true;
								qsbLightSensor.IlluminatedByLocal = true;
								break;
							}
						}

						break;
					default:
						if (lightSourceType == LightSourceType.VOLUME_ONLY)
						{
							__instance._illuminated = true;
							qsbLightSensor.IlluminatedByLocal = true;
						}

						break;
				}
			}
		}

		if (qsbLightSensor.IlluminatedByLocal && !illuminatedByLocal)
		{
			qsbLightSensor.OnDetectLocalLight?.Invoke();
		}
		else if (!qsbLightSensor.IlluminatedByLocal && illuminatedByLocal)
		{
			qsbLightSensor.OnDetectLocalDarkness?.Invoke();
		}

		return false;
	}
}
