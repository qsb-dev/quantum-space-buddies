using HarmonyLib;
using QSB.EchoesOfTheEye.LightSensorSync.Messages;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.Tools.FlashlightTool;
using QSB.Tools.ProbeTool;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.LightSensorSync.Patches;

[HarmonyPatch(typeof(SingleLightSensor))]
internal class LightSensorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SingleLightSensor.OnSectorOccupantsUpdated))]
	private static bool OnSectorOccupantsUpdated(SingleLightSensor __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var flag = __instance._sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (flag && !__instance.enabled)
		{
			__instance.enabled = true;
			__instance.GetWorldObject<QSBLightSensor>().SendMessage(new SetEnabledMessage(true));
			__instance._lightDetector.GetShape().enabled = true;
			if (__instance._preserveStateWhileDisabled)
			{
				__instance._fixedUpdateFrameDelayCount = 10;
			}
		}
		else if (!flag && __instance.enabled)
		{
			__instance.enabled = false;
			__instance.GetWorldObject<QSBLightSensor>().SendMessage(new SetEnabledMessage(false));
			__instance._lightDetector.GetShape().enabled = false;
			if (!__instance._preserveStateWhileDisabled)
			{
				if (__instance._illuminated)
				{
					__instance.OnDetectDarkness.Invoke();
				}

				__instance._illuminated = false;
			}
		}

		return false;
	}

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
							if (lightSource is Flashlight)
							{
								var position = Locator.GetPlayerCamera().transform.position;
								var to = __instance.transform.position - position;
								if (Vector3.Angle(Locator.GetPlayerCamera().transform.forward, to) <= __instance._maxSpotHalfAngle
									&& !__instance.CheckOcclusion(position, vector, sensorWorldDir))
								{
									__instance._illuminated = true;
									qsbLightSensor.IlluminatedByLocal = true;
								}
							}
							else if (lightSource is QSBFlashlight qsbFlashlight)
							{
								var playerCamera = qsbFlashlight.Player.Camera;

								var position = playerCamera.transform.position;
								var to = __instance.transform.position - position;
								if (Vector3.Angle(playerCamera.transform.forward, to) <= __instance._maxSpotHalfAngle
									&& !__instance.CheckOcclusion(position, vector, sensorWorldDir))
								{
									__instance._illuminated = true;
								}
							}

							break;
						}
					case LightSourceType.PROBE:
						{
							if (lightSource is SurveyorProbe probe)
							{
								if (probe != null
									&& probe.IsLaunched()
									&& !probe.IsRetrieving()
									&& probe.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance)
									&& !__instance.CheckOcclusion(probe.GetLightSourcePosition(), vector, sensorWorldDir))
								{
									__instance._illuminated = true;
									qsbLightSensor.IlluminatedByLocal = true;
								}
							}
							else if (lightSource is QSBProbe qsbProbe)
							{
								if (qsbProbe != null
									&& qsbProbe.IsLaunched()
									&& !qsbProbe.IsRetrieving()
									&& qsbProbe.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance)
									&& !__instance.CheckOcclusion(qsbProbe.GetLightSourcePosition(), vector, sensorWorldDir))
								{
									__instance._illuminated = true;
								}
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
								qsbLightSensor.IlluminatedByLocal = true; // todo remote dream lanterns
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
