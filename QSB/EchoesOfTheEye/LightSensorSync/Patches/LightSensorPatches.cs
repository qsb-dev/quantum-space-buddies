using HarmonyLib;
using QSB.AuthoritySync;
using QSB.EchoesOfTheEye.LightSensorSync.Messages;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Tools.FlashlightTool;
using QSB.Tools.ProbeTool;
using QSB.Utility;
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
	[HarmonyPatch(nameof(SingleLightSensor.OnSectorOccupantsUpdated))]
	private static bool OnSectorOccupantsUpdated(SingleLightSensor __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}
		var qsbLightSensor = __instance.GetWorldObject<QSBLightSensor>();

		var containsAnyOccupants = __instance._sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (containsAnyOccupants && !__instance.enabled)
		{
			__instance.enabled = true;
			__instance._lightDetector.GetShape().enabled = true;
			qsbLightSensor.RequestOwnership();
			if (__instance._preserveStateWhileDisabled)
			{
				__instance._fixedUpdateFrameDelayCount = 10;
			}
		}
		else if (!containsAnyOccupants && __instance.enabled)
		{
			__instance.enabled = false;
			__instance._lightDetector.GetShape().enabled = false;
			qsbLightSensor.ReleaseOwnership();
			if (!__instance._preserveStateWhileDisabled)
			{
				if (__instance._illuminated)
				{
					Delay.RunFramesLater(10, () =>
					{
						// no one else took ownership, so we can safely make not illuminated
						// ie turn off when no one else is there
						if (qsbLightSensor.Owner == 0)
						{
							__instance._illuminated = false;
							__instance.OnDetectDarkness.Invoke();
							qsbLightSensor.SendMessage(new SetIlluminatedMessage(false));
						}
					});
				}

				if (qsbLightSensor._locallyIlluminated)
				{
					qsbLightSensor._locallyIlluminated = false;
					qsbLightSensor.OnDetectLocalDarkness?.Invoke();
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
		if (LightSensorManager.IsPlayerLightSensor(__instance))
		{
			return true;
		}
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}
		var qsbLightSensor = __instance.GetWorldObject<QSBLightSensor>();

		if (__instance._fixedUpdateFrameDelayCount > 0)
		{
			__instance._fixedUpdateFrameDelayCount--;
			return false;
		}
		if (qsbLightSensor.Owner == QSBPlayerManager.LocalPlayerId)
		{
			var illuminated = __instance._illuminated;
			if (__instance._illuminatingDreamLanternList != null)
			{
				_illuminatingDreamLanternList.Clear();
				_illuminatingDreamLanternList.AddRange(__instance._illuminatingDreamLanternList);
			}
			__instance.UpdateIllumination();
			if (__instance._illuminatingDreamLanternList != null &&
				!__instance._illuminatingDreamLanternList.SequenceEqual(_illuminatingDreamLanternList))
			{
				qsbLightSensor.SendMessage(new IlluminatingLanternsMessage(__instance._illuminatingDreamLanternList));
			}
			if (!illuminated && __instance._illuminated)
			{
				__instance.OnDetectLight.Invoke();
				qsbLightSensor.SendMessage(new SetIlluminatedMessage(true));
			}
			else if (illuminated && !__instance._illuminated)
			{
				__instance.OnDetectDarkness.Invoke();
				qsbLightSensor.SendMessage(new SetIlluminatedMessage(false));
			}
		}

		var locallyIlluminated = qsbLightSensor._locallyIlluminated;
		UpdateLocalIllumination(__instance);
		if (!locallyIlluminated && qsbLightSensor._locallyIlluminated)
		{
			qsbLightSensor.OnDetectLocalLight?.Invoke();
		}
		else if (locallyIlluminated && !qsbLightSensor._locallyIlluminated)
		{
			qsbLightSensor.OnDetectLocalDarkness?.Invoke();
		}
		return false;
	}


	#region player light sensor

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SingleLightSensor.ManagedFixedUpdate))]
	private static bool Player_ManagedFixedUpdate(SingleLightSensor __instance)
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
		if (__instance._illuminatingDreamLanternList != null &&
			!__instance._illuminatingDreamLanternList.SequenceEqual(_illuminatingDreamLanternList))
		{
			new PlayerIlluminatingLanternsMessage(QSBPlayerManager.LocalPlayerId, __instance._illuminatingDreamLanternList).Send();
		}
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
		return false;
	}

	#endregion


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
							if (lightSource is QSBSurveyorProbe qsbProbe)
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

	private static void UpdateLocalIllumination(SingleLightSensor __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}
		var qsbLightSensor = __instance.GetWorldObject<QSBLightSensor>();

		qsbLightSensor._locallyIlluminated = false;
		if (__instance._lightSources == null || __instance._lightSources.Count == 0)
		{
			return;
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
					case LightSourceType.FLASHLIGHT:
						{
							if (lightSource is not QSBFlashlight)
							{
								var position = Locator.GetPlayerCamera().transform.position;
								var vector3 = __instance.transform.position - position;
								if (Vector3.Angle(Locator.GetPlayerCamera().transform.forward, vector3) <= __instance._maxSpotHalfAngle &&
									!__instance.CheckOcclusion(position, sensorWorldPos, sensorWorldDir))
								{
									qsbLightSensor._locallyIlluminated = true;
								}
							}
							break;
						}
					case LightSourceType.PROBE:
						{
							if (lightSource is not QSBSurveyorProbe)
							{
								var probe = Locator.GetProbe();
								if (probe != null &&
									probe.IsLaunched() &&
									!probe.IsRetrieving() &&
									probe.CheckIlluminationAtPoint(sensorWorldPos, __instance._sensorRadius, __instance._maxDistance) &&
									!__instance.CheckOcclusion(probe.GetLightSourcePosition(), sensorWorldPos, sensorWorldDir))
								{
									qsbLightSensor._locallyIlluminated = true;
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
								var dreamLanternItem = dreamLanternController.GetComponent<DreamLanternItem>();
								qsbLightSensor._locallyIlluminated |= QSBPlayerManager.LocalPlayer.HeldItem?.AttachedObject == dreamLanternItem;
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
								var simpleLanternItem = (SimpleLanternItem)lightSource;
								qsbLightSensor._locallyIlluminated |= QSBPlayerManager.LocalPlayer.HeldItem?.AttachedObject == simpleLanternItem;
							}
						}
						break;
				}
			}
		}
	}
}
