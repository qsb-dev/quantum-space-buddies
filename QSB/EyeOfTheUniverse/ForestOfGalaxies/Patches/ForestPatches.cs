using HarmonyLib;
using QSB.Patches;
using QSB.Player;
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
	}
}
