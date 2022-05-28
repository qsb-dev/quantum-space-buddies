using HarmonyLib;
using QSB.EchoesOfTheEye.LightSensorSync.Messages;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
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
				qsbLightSensor.OnDetectLocalLight?.Invoke();
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
					qsbLightSensor.OnDetectLocalDarkness?.Invoke();
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
		}

		var illuminated = __instance._illuminated;
		__instance.UpdateIllumination();
		var locallyIlluminated = qsbLightSensor.LocallyIlluminated;
		qsbLightSensor.LocallyIlluminated = __instance._illuminated;
		__instance._illuminated = illuminated;

		if (!locallyIlluminated && qsbLightSensor.LocallyIlluminated)
		{
			qsbLightSensor.LocallyIlluminated = true;
			qsbLightSensor.OnDetectLocalLight?.Invoke();
			qsbLightSensor.SendMessage(new SetIlluminatedMessage(true));
		}
		else if (locallyIlluminated && !qsbLightSensor.LocallyIlluminated)
		{
			qsbLightSensor.LocallyIlluminated = false;
			qsbLightSensor.OnDetectLocalDarkness?.Invoke();
			qsbLightSensor.SendMessage(new SetIlluminatedMessage(false));
		}

		return false;
	}
}
