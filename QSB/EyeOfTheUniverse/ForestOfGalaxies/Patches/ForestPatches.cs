using HarmonyLib;
using QSB.EyeOfTheUniverse.ForestOfGalaxies.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.ForestOfGalaxies.Patches
{
	internal class ForestPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(OldGrowthForestController), nameof(OldGrowthForestController.CheckIllumination))]
		public static bool CheckIlluminationReplacement(Vector3 worldPosition, ref bool __result)
		{
			if (Locator.GetFlashlight().IsFlashlightOn() || QSBPlayerManager.PlayerList.Any(x => x.FlashlightActive))
			{
				foreach (var player in QSBPlayerManager.PlayerList)
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
				|| QSBPlayerManager.PlayerList.Where(x => x != QSBPlayerManager.LocalPlayer).Any(x => x.Probe != null && x.Probe.IsAnchored()))
			{
				foreach (var player in QSBPlayerManager.PlayerList)
				{
					if (player == QSBPlayerManager.LocalPlayer)
					{
						var vector = Locator.GetProbe().transform.position - worldPosition;
						vector.y = 0f;
						if (vector.magnitude < 50f)
						{
							__result = true;
							return false;
						}
					}
					else
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

			__result = true;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MiniGalaxyController), nameof(MiniGalaxyController.KillGalaxies))]
		public static bool KillGalaxiesReplacement(MiniGalaxyController __instance)
		{
			var num = 60f;
			__instance._galaxies = __instance.GetComponentsInChildren<MiniGalaxy>(true);
			var delayList = new List<float>();
			for (var i = 0; i < __instance._galaxies.Length; i++)
			{
				var rnd = Random.Range(30f, num);
				delayList.Add(rnd);
				__instance._galaxies[i].DieAfterSeconds(rnd, true, AudioType.EyeGalaxyBlowAway);
			}
			new KillGalaxiesMessage(delayList).Send();

			__instance._forestIsDarkTime = Time.time + num + 5f;
			__instance.enabled = true;

			return false;
		}
	}
}
