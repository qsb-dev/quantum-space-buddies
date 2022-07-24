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
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var isPlayerLightSensor = LightSensorManager.IsPlayerLightSensor(__instance);
		var qsbPlayerLightSensor = isPlayerLightSensor ? __instance.GetComponent<QSBPlayerLightSensor>() : null;
		var qsbLightSensor = isPlayerLightSensor ? null : __instance.GetWorldObject<QSBLightSensor>();

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
				if (isPlayerLightSensor)
				{
					qsbPlayerLightSensor._locallyIlluminated = true;
					new PlayerSetIlluminatedMessage(qsbPlayerLightSensor.PlayerId, true).Send();
				}
				else
				{
					qsbLightSensor._locallyIlluminated = true;
					qsbLightSensor.OnDetectLocalLight?.Invoke();
					qsbLightSensor.SendMessage(new SetIlluminatedMessage(true));
				}
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

		var isPlayerLightSensor = LightSensorManager.IsPlayerLightSensor(__instance);
		var qsbPlayerLightSensor = isPlayerLightSensor ? __instance.GetComponent<QSBPlayerLightSensor>() : null;
		var qsbLightSensor = isPlayerLightSensor ? null : __instance.GetWorldObject<QSBLightSensor>();

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
				if (isPlayerLightSensor)
				{
					if (qsbPlayerLightSensor._locallyIlluminated)
					{
						qsbPlayerLightSensor._locallyIlluminated = false;
						new PlayerSetIlluminatedMessage(qsbPlayerLightSensor.PlayerId, false).Send();
					}
				}
				else
				{
					if (qsbLightSensor._locallyIlluminated)
					{
						qsbLightSensor._locallyIlluminated = false;
						qsbLightSensor.OnDetectLocalDarkness?.Invoke();
						qsbLightSensor.SendMessage(new SetIlluminatedMessage(false));
					}
				}
			}
		}

		return false;
	}

	/// <summary>
	/// to prevent allocating a new list every frame
	/// </summary>
	private static readonly List<DreamLanternController> _prevIlluminatingDreamLanternList = new();

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
			_prevIlluminatingDreamLanternList.Clear();
			_prevIlluminatingDreamLanternList.AddRange(__instance._illuminatingDreamLanternList);
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
			&& !__instance._illuminatingDreamLanternList.SequenceEqual(_prevIlluminatingDreamLanternList))
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
}
