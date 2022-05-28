using HarmonyLib;
using QSB.EchoesOfTheEye.LightSensorSync.Messages;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.EchoesOfTheEye.LightSensorSync.Patches;

[HarmonyPatch(typeof(SingleLightSensor))]
internal class LightSensorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SingleLightSensor.Start))]
	private static bool Start(SingleLightSensor __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		if (!__instance.TryGetWorldObject(out QSBLightSensor qsbLightSensor))
		{
			return true;
		}

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
			if (__instance._startIlluminated)
			{
				qsbLightSensor.LocallyIlluminated = true;
				DebugLog.DebugWrite($"{qsbLightSensor} LocallyIlluminated = true");
				qsbLightSensor.OnDetectLocalLight?.Invoke();

				qsbLightSensor._clientIlluminated = true;
				DebugLog.DebugWrite($"{qsbLightSensor} _clientIlluminated = true");
				qsbLightSensor.SendMessage(new SetIlluminatedMessage(true));
			}
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

		if (!__instance.TryGetWorldObject(out QSBLightSensor qsbLightSensor))
		{
			return true;
		}

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
				if (qsbLightSensor.LocallyIlluminated)
				{
					qsbLightSensor.LocallyIlluminated = false;
					DebugLog.DebugWrite($"{qsbLightSensor} LocallyIlluminated = false");
					qsbLightSensor.OnDetectLocalDarkness?.Invoke();
				}

				if (qsbLightSensor._clientIlluminated)
				{
					qsbLightSensor._clientIlluminated = false;
					DebugLog.DebugWrite($"{qsbLightSensor} _clientIlluminated = false");
					qsbLightSensor.SendMessage(new SetIlluminatedMessage(false));
				}
			}
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SingleLightSensor.ManagedFixedUpdate))]
	private static bool ManagedFixedUpdate(SingleLightSensor __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		if (!__instance.TryGetWorldObject(out QSBLightSensor qsbLightSensor))
		{
			return true;
		}

		if (__instance._fixedUpdateFrameDelayCount > 0)
		{
			__instance._fixedUpdateFrameDelayCount--;
			return false;
		}

		var locallyIlluminated = qsbLightSensor.LocallyIlluminated;
		var clientIlluminated = qsbLightSensor._clientIlluminated;
		__instance.UpdateIllumination();
		if (!locallyIlluminated && qsbLightSensor.LocallyIlluminated)
		{
			DebugLog.DebugWrite($"{qsbLightSensor} LocallyIlluminated = true");
			qsbLightSensor.OnDetectLocalLight?.Invoke();
		}
		else if (locallyIlluminated && !qsbLightSensor.LocallyIlluminated)
		{
			DebugLog.DebugWrite($"{qsbLightSensor} LocallyIlluminated = false");
			qsbLightSensor.OnDetectLocalDarkness?.Invoke();
		}

		if (!clientIlluminated && qsbLightSensor._clientIlluminated)
		{
			DebugLog.DebugWrite($"{qsbLightSensor} _clientIlluminated = true");
			qsbLightSensor.SendMessage(new SetIlluminatedMessage(true));
		}
		else if (clientIlluminated && !qsbLightSensor._clientIlluminated)
		{
			DebugLog.DebugWrite($"{qsbLightSensor} _clientIlluminated = false");
			qsbLightSensor.SendMessage(new SetIlluminatedMessage(false));
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

		if (!__instance.TryGetWorldObject(out QSBLightSensor qsbLightSensor))
		{
			return true;
		}

		qsbLightSensor.LocallyIlluminated = false;
		qsbLightSensor._clientIlluminated = false;
		__instance._illuminatingDreamLanternList?.Clear();
		if (__instance._lightSources == null || __instance._lightSources.Count == 0)
		{
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
			if ((__instance._lightSourceMask & lightSource.GetLightSourceType()) == lightSource.GetLightSourceType()
				&& lightSource.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance))
			{
				switch (lightSource.GetLightSourceType())
				{
					case LightSourceType.UNDEFINED:
						{
							var owlight = lightSource as OWLight2;
							var occludableLight = owlight.GetLight().shadows != LightShadows.None && owlight.GetLight().shadowStrength > 0.5f;
							if (owlight.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance)
								&& !__instance.CheckOcclusion(owlight.transform.position, vector, sensorWorldDir, occludableLight))
							{
								qsbLightSensor._clientIlluminated = true;
							}

							break;
						}
					case LightSourceType.FLASHLIGHT:
						{
							var position = Locator.GetPlayerCamera().transform.position;
							var to = __instance.transform.position - position;
							if (Vector3.Angle(Locator.GetPlayerCamera().transform.forward, to) <= __instance._maxSpotHalfAngle
								&& !__instance.CheckOcclusion(position, vector, sensorWorldDir))
							{
								qsbLightSensor.LocallyIlluminated |= lightSource is Flashlight;
								qsbLightSensor._clientIlluminated = true;
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
								qsbLightSensor.LocallyIlluminated |= lightSource is SurveyorProbe;
								qsbLightSensor._clientIlluminated = true;
							}

							break;
						}
					case LightSourceType.DREAM_LANTERN:
						{
							var dreamLanternController = lightSource as DreamLanternController;
							if (dreamLanternController.IsLit()
								&& dreamLanternController.IsFocused(__instance._lanternFocusThreshold)
								&& dreamLanternController.CheckIlluminationAtPoint(vector, __instance._sensorRadius, __instance._maxDistance)
								&& !__instance.CheckOcclusion(dreamLanternController.GetLightPosition(), vector, sensorWorldDir))
							{
								__instance._illuminatingDreamLanternList.Add(dreamLanternController);
								var dreamLanternItem = dreamLanternController.GetComponent<DreamLanternItem>();
								qsbLightSensor.LocallyIlluminated |= QSBPlayerManager.LocalPlayer.HeldItem?.AttachedObject == dreamLanternItem;
								qsbLightSensor._clientIlluminated = true;
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
								var simpleLanternItem = (SimpleLanternItem)lightSource;
								qsbLightSensor.LocallyIlluminated |= QSBPlayerManager.LocalPlayer.HeldItem?.AttachedObject == simpleLanternItem;
								qsbLightSensor._clientIlluminated = true;
								break;
							}
						}

						break;
					case LightSourceType.VOLUME_ONLY:
						qsbLightSensor._clientIlluminated = true;
						break;
				}
			}
		}

		return false;
	}
}
