using HarmonyLib;
using QSB.EchoesOfTheEye.AlarmTotemSync.Messages;
using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Patches;

public class AlarmTotemPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

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
			var lanternController = player.AssignedSimulationLantern?.AttachedObject?._lanternController;
			if (!lanternController)
			{
				continue;
			}
			var playerLightSensor = player.LightSensor;
			if ((lanternController.IsHeldByPlayer() && !lanternController.IsConcealed()) || playerLightSensor.IsIlluminated())
			{
				var position = player.Camera.transform.position;
				if (__instance.CheckPointInVisionCone(position) && !__instance.CheckLineOccluded(__instance._sightOrigin.position, position))
				{
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
