using HarmonyLib;
using QSB.AuthoritySync;
using QSB.EchoesOfTheEye.AlarmTotemSync.Messages;
using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Patches;

public class AlarmTotemPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmTotem), nameof(AlarmTotem.OnSectorOccupantAdded))]
	private static bool OnSectorOccupantAdded(AlarmTotem __instance, SectorDetector sectorDetector)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			__instance.enabled = true;
			var qsbAlarmTotem = __instance.GetWorldObject<QSBAlarmTotem>();
			qsbAlarmTotem.RequestOwnership();
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmTotem), nameof(AlarmTotem.OnSectorOccupantRemoved))]
	private static bool OnSectorOccupantRemoved(AlarmTotem __instance, SectorDetector sectorDetector)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			__instance.enabled = false;
			var qsbAlarmTotem = __instance.GetWorldObject<QSBAlarmTotem>();
			qsbAlarmTotem.ReleaseOwnership();
			Delay.RunFramesLater(10, () =>
			{
				// no one else took ownership, so we can safely turn stuff off
				// ie turn off when no one else is there
				if (qsbAlarmTotem.Owner == 0)
				{
					__instance._pulseLightController.SetIntensity(0f);
					__instance._simTotemMaterials[0] = __instance._origSimEyeMaterial;
					__instance._simTotemRenderer.sharedMaterials = __instance._simTotemMaterials;
					__instance._simVisionConeRenderer.SetColor(__instance._simVisionConeRenderer.GetOriginalColor());
					if (__instance._isPlayerVisible)
					{
						__instance._isPlayerVisible = false;
						__instance._secondsConcealed = 0f;
						Locator.GetAlarmSequenceController().DecreaseAlarmCounter();
					}
				}
			});
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmTotem), nameof(AlarmTotem.FixedUpdate))]
	private static bool FixedUpdate(AlarmTotem __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var qsbAlarmTotem = __instance.GetWorldObject<QSBAlarmTotem>();

		if (qsbAlarmTotem.Owner != QSBPlayerManager.LocalPlayerId)
		{
			return false;
		}

		var isPlayerVisible = __instance._isPlayerVisible;
		__instance._isPlayerVisible = __instance.CheckPlayerVisible();
		if (!isPlayerVisible && __instance._isPlayerVisible)
		{
			Locator.GetAlarmSequenceController().IncreaseAlarmCounter();
			__instance._secondsConcealed = 0f;
			__instance._simTotemMaterials[0] = __instance._simAlarmMaterial;
			__instance._simTotemRenderer.sharedMaterials = __instance._simTotemMaterials;
			__instance._simVisionConeRenderer.SetColor(__instance._simAlarmColor);
			GlobalMessenger.FireEvent("AlarmTotemTriggered");
			qsbAlarmTotem.SendMessage(new SetVisibleMessage(true));
		}
		else if (isPlayerVisible && !__instance._isPlayerVisible)
		{
			Locator.GetAlarmSequenceController().DecreaseAlarmCounter();
			__instance._secondsConcealed = 0f;
			__instance._simTotemMaterials[0] = __instance._origSimEyeMaterial;
			__instance._simTotemRenderer.sharedMaterials = __instance._simTotemMaterials;
			__instance._simVisionConeRenderer.SetColor(__instance._simVisionConeRenderer.GetOriginalColor());
			__instance._pulseLightController.FadeTo(0f, 0.5f);
			qsbAlarmTotem.SendMessage(new SetVisibleMessage(false));
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmTotem), nameof(AlarmTotem.CheckPlayerVisible))]
	private static bool CheckPlayerVisible(AlarmTotem __instance, out bool __result)
	{
		if (!__instance._isFaceOpen)
		{
			__result = false;
			return false;
		}

		foreach (var player in QSBPlayerManager.PlayerList)
		{
			var position = player.Camera.transform.position;
			if (__instance.CheckPointInVisionCone(position) && !__instance.CheckLineOccluded(__instance._sightOrigin.position, position))
			{
				if (player.LightSensor.IsIlluminated())
				{
					__result = true;
					return false;
				}

				if (player.AssignedSimulationLantern == null)
				{
					continue;
				}

				var lanternController = player.AssignedSimulationLantern.AttachedObject.GetLanternController();
				if (lanternController.IsHeldByPlayer())
				{
					if (lanternController.IsConcealed())
					{
						if (!__instance._hasConcealedFromAlarm)
						{
							__instance._secondsConcealed += Time.deltaTime;
							if (__instance._secondsConcealed > 1f)
							{
								__instance._hasConcealedFromAlarm = true;
								GlobalMessenger.FireEvent("ConcealFromAlarmTotem");
							}
						}

						__result = false;
						return false;
					}

					__result = true;
					return false;
				}
			}
		}

		__result = false;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmBell), nameof(AlarmBell.OnEntry))]
	private static bool OnEntry(AlarmBell __instance, GameObject hitObj)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		if (hitObj.CompareTag("ProbeDetector"))
		{
			__instance._oneShotSource.PlayOneShot(AudioType.AlarmChime_RW);

			__instance.GetWorldObject<QSBAlarmBell>()
				.SendMessage(new BellHitMessage(1));
		}
		else if (hitObj.CompareTag("PlayerDetector"))
		{
			var vector = __instance.gameObject.GetAttachedOWRigidbody().GetPointVelocity(__instance._bellTrigger.transform.position) - Locator.GetPlayerBody().GetVelocity();
			var magnitude = Vector3.ProjectOnPlane(vector, __instance._bellTrigger.transform.up).magnitude;
			if (magnitude > 4f)
			{
				var volume = Mathf.Lerp(0.2f, 1f, Mathf.InverseLerp(4f, 12f, magnitude));
				__instance._oneShotSource.PlayOneShot(AudioType.AlarmChime_RW, volume);

				__instance.GetWorldObject<QSBAlarmBell>()
					.SendMessage(new BellHitMessage(volume));
			}
		}

		return false;
	}
}
