using HarmonyLib;
using QSB.Patches;
using QSB.Player;
using QSB.Tools.FlashlightTool;
using System.Linq;
using UnityEngine;

namespace QSB.EchoesOfTheEye.LightSensorSync.Patches
{
	[HarmonyPatch]
	internal class LightSensorPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SingleLightSensor), nameof(SingleLightSensor.UpdateIllumination))]
		public static bool UpdateIlluminationReplacement(SingleLightSensor __instance)
		{
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
								var owlight = source as OWLight2;
								var occludableLight = owlight.GetLight().shadows != LightShadows.None
									&& owlight.GetLight().shadowStrength > 0.5f;

								if (owlight.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance)
									&& !__instance.CheckOcclusion(owlight.transform.position, vector, sensorWorldDir, occludableLight))
								{
									__instance._illuminated = true;
								}

								break;
							}
						case LightSourceType.FLASHLIGHT:
							{
								if (source is Flashlight && (source as Flashlight) == Locator.GetFlashlight())
								{
									var position = Locator.GetPlayerCamera().transform.position;
									var to = __instance.transform.position - position;
									if (Vector3.Angle(Locator.GetPlayerCamera().transform.forward, to) <= __instance._maxSpotHalfAngle
										&& !__instance.CheckOcclusion(position, vector, sensorWorldDir, true))
									{
										__instance._illuminated = true;
									}
								}
								else
								{
									var player = QSBPlayerManager.PlayerList.First(x => x.FlashLight == (QSBFlashlight)source);

									var position = player.Camera.transform.position;
									var to = __instance.transform.position - position;
									if (Vector3.Angle(player.Camera.transform.forward, to) <= __instance._maxSpotHalfAngle
										&& !__instance.CheckOcclusion(position, vector, sensorWorldDir, true))
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
									&& !__instance.CheckOcclusion(probe.GetLightSourcePosition(), vector, sensorWorldDir, true))
								{
									__instance._illuminated = true;
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
									&& !__instance.CheckOcclusion(dreamLanternController.GetLightPosition(), vector, sensorWorldDir, true))
								{
									__instance._illuminatingDreamLanternList.Add(dreamLanternController);
									__instance._illuminated = true;
								}

								break;
							}
						case LightSourceType.SIMPLE_LANTERN:
							foreach (var owlight in __instance._lightSources[i].GetLights())
							{
								var occludableLight = owlight.GetLight().shadows != LightShadows.None
									&& owlight.GetLight().shadowStrength > 0.5f;
								var maxDistance = Mathf.Min(__instance._maxSimpleLanternDistance, __instance._maxDistance);
								if (owlight.CheckIlluminationAtPoint(vector, __instance._sensorRadius, maxDistance) && !__instance.CheckOcclusion(owlight.transform.position, vector, sensorWorldDir, occludableLight))
								{
									__instance._illuminated = true;
									break;
								}
							}
							break;
						default:
							if (lightSourceType == LightSourceType.VOLUME_ONLY)
							{
								__instance._illuminated = true;
							}
							break;
					}
				}
			}

			return false;
		}
	}
}
