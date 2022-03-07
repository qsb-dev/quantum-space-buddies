using HarmonyLib;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Patches;
using QSB.Player;
using QSB.Tools.FlashlightTool;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.EchoesOfTheEye.LightSensorSync.Patches;

[HarmonyPatch]
internal class LightSensorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SingleLightSensor), nameof(SingleLightSensor.UpdateIllumination))]
	public static bool UpdateIlluminationReplacement(SingleLightSensor __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var qsbLightSensor = __instance.GetWorldObject<QSBLightSensor>();
		qsbLightSensor._illuminatedByLocal = false;
		__instance._illuminated = false;
		if (__instance._illuminatingDreamLanternList != null)
		{
			__instance._illuminatingDreamLanternList.Clear();
		}

		var vector = __instance.transform.TransformPoint(__instance._localSensorOffset);
		var sensorWorldDir = Vector3.zero;
		if (__instance._directionalSensor)
		{
			sensorWorldDir = __instance.transform.TransformDirection(__instance._localDirection).normalized;
		}

		if (__instance._lightSources == null)
		{
			return false;
		}

		for (var i = 0; i < __instance._lightSources.Count; i++)
		{
			var source = __instance._lightSources[i];

			if ((__instance._lightSourceMask & source.GetLightSourceType()) == source.GetLightSourceType()
			    && source.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance))
			{
				var lightSourceType = source.GetLightSourceType();
				switch (lightSourceType)
				{
					case LightSourceType.UNDEFINED:
						{
							var light = source as OWLight2;
							var occludableLight = light.GetLight().shadows != LightShadows.None
							                      && light.GetLight().shadowStrength > 0.5f;

							if (light.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance)
							    && !__instance.CheckOcclusion(light.transform.position, vector, sensorWorldDir, occludableLight))
							{
								__instance._illuminated = true;
								qsbLightSensor._illuminatedByLocal = true;
							}

							break;
						}
					case LightSourceType.FLASHLIGHT:
						{
							if (Locator.GetFlashlight() == source as Flashlight)
							{
								var position = Locator.GetPlayerCamera().transform.position;
								var to = __instance.transform.position - position;
								if (Vector3.Angle(Locator.GetPlayerCamera().transform.forward, to) <= __instance._maxSpotHalfAngle
								    && !__instance.CheckOcclusion(position, vector, sensorWorldDir))
								{
									__instance._illuminated = true;
									qsbLightSensor._illuminatedByLocal = true;
								}
							}
							else
							{
								var player = QSBPlayerManager.PlayerList.First(x => x.FlashLight == source as QSBFlashlight);

								var position = player.Camera.transform.position;
								var to = __instance.transform.position - position;
								if (Vector3.Angle(player.Camera.transform.forward, to) <= __instance._maxSpotHalfAngle
								    && !__instance.CheckOcclusion(position, vector, sensorWorldDir))
								{
									__instance._illuminated = true;
								}
							}

							break;
						}
					case LightSourceType.PROBE:
						{
							var probe = Locator.GetProbe();
							if (probe != null
							    && probe.IsLaunched()
							    && !probe.IsRetrieving()
							    && probe.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance)
							    && !__instance.CheckOcclusion(probe.GetLightSourcePosition(), vector, sensorWorldDir))
							{
								__instance._illuminated = true;
								qsbLightSensor._illuminatedByLocal = true;
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
							var dreamLanternController = __instance._lightSources[i] as DreamLanternController;
							if (dreamLanternController.IsLit()
							    && dreamLanternController.IsFocused(__instance._lanternFocusThreshold)
							    && dreamLanternController.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance)
							    && !__instance.CheckOcclusion(dreamLanternController.GetLightPosition(), vector, sensorWorldDir))
							{
								__instance._illuminatingDreamLanternList.Add(dreamLanternController);
								__instance._illuminated = true;
								qsbLightSensor._illuminatedByLocal = true;
							}

							break;
						}
					case LightSourceType.SIMPLE_LANTERN:
						foreach (var light in __instance._lightSources[i].GetLights())
						{
							var occludableLight = light.GetLight().shadows != LightShadows.None
							                      && light.GetLight().shadowStrength > 0.5f;
							var maxDistance = Mathf.Min(__instance._maxSimpleLanternDistance, __instance._maxDistance);
							if (light.CheckIlluminationAtPoint(vector, __instance._sensorRadius, maxDistance) && !__instance.CheckOcclusion(light.transform.position, vector, sensorWorldDir, occludableLight))
							{
								__instance._illuminated = true;
								qsbLightSensor._illuminatedByLocal = true;
								break;
							}
						}

						break;
					default:
						if (lightSourceType == LightSourceType.VOLUME_ONLY)
						{
							__instance._illuminated = true;
							qsbLightSensor._illuminatedByLocal = true;
						}

						break;
				}
			}
		}

		return false;
	}
}