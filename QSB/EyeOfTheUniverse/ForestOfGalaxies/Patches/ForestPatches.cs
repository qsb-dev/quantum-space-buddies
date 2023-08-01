using HarmonyLib;
using QSB.EyeOfTheUniverse.ForestOfGalaxies.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.ForestOfGalaxies.Patches;

public class ForestPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(OldGrowthForestController), nameof(OldGrowthForestController.CheckIllumination))]
	public static bool CheckIlluminationReplacement(Vector3 worldPosition, out bool __result)
	{
		if (Locator.GetFlashlight().IsFlashlightOn() || QSBPlayerManager.PlayerList.Any(x => x.FlashlightActive))
		{
			foreach (var player in QSBPlayerManager.PlayerList.Where(x => x.FlashlightActive))
			{
				var vector = player.Body.transform.position - worldPosition;
				vector.y = 0f;
				if (vector.magnitude < 50f)
				{
					__result = true;
					return false;
				}
			}
		}

		if ((Locator.GetProbe() != null && Locator.GetProbe().IsAnchored())
		    || QSBPlayerManager.PlayerList.Where(x => !x.IsLocalPlayer).Any(x => x.Probe != null && x.Probe.IsAnchored()))
		{
			foreach (var player in QSBPlayerManager.PlayerList)
			{
				if (player.IsLocalPlayer
				    && Locator.GetProbe() != null
				    && Locator.GetProbe().IsAnchored())
				{
					var vector = Locator.GetProbe().transform.position - worldPosition;
					vector.y = 0f;
					if (vector.magnitude < 50f)
					{
						__result = true;
						return false;
					}
				}
				else if (player.Probe != null && player.Probe.IsAnchored())
				{
					var vector = player.ProbeBody.transform.position - worldPosition;
					vector.y = 0f;
					if (vector.magnitude < 50f)
					{
						__result = true;
						return false;
					}
				}
			}
		}

		__result = false;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MiniGalaxyController), nameof(MiniGalaxyController.KillGalaxies))]
	public static bool KillGalaxiesReplacement(MiniGalaxyController __instance)
	{
		const float delay = 60f;
		__instance._galaxies = __instance.GetComponentsInChildren<MiniGalaxy>(true);
		var delayList = new List<float>();
		foreach (var galaxy in __instance._galaxies)
		{
			var rnd = Random.Range(30f, delay);
			delayList.Add(rnd);
			galaxy.DieAfterSeconds(rnd, true, AudioType.EyeGalaxyBlowAway);
		}

		new KillGalaxiesMessage(delayList).Send();
		__instance._forestIsDarkTime = Time.time + delay + 5f;
		__instance.enabled = true;

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(PlayerCloneController), nameof(PlayerCloneController.FixedUpdate))]
	public static bool CloneFixedUpdate(PlayerCloneController __instance)
	{
		var playerTransform = Locator.GetPlayerTransform();
		var vector = __instance.transform.parent.InverseTransformPoint(playerTransform.position);
		var b = __instance._localMirrorPos - vector;
		var position = __instance._localMirrorPos + b;
		position.y = vector.y;
		__instance.transform.position = __instance.transform.parent.TransformPoint(position);
		var normalized = (__instance.transform.position - playerTransform.position).normalized;
		var forward = Vector3.Reflect(playerTransform.forward, normalized);
		var upwards = Vector3.Reflect(playerTransform.up, normalized);
		__instance.transform.rotation = Quaternion.LookRotation(forward, upwards);
		var num = Vector3.Distance(__instance.transform.position, playerTransform.position);
		if (!__instance._warpFlickerActivated && num < 10f)
		{
			__instance._warpFlickerActivated = true;
			__instance._warpTime = Time.time + 0.5f;
			GlobalMessenger<float, float>.FireEvent(OWEvents.FlickerOffAndOn, 0.5f, 0.5f);
			new EyeCloneSeenMessage().Send();
		}

		if (__instance._warpPlayerNextFrame)
		{
			__instance.WarpPlayerToCampfire();
		}

		return false;
	}
}