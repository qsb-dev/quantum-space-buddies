using HarmonyLib;
using QSB.Patches;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EyeOfTheUniverse.CosmicInflation.Patches
{
	internal class InflationPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CosmicInflationController), nameof(CosmicInflationController.StartCollapse))]
		public static bool StartCollapse(CosmicInflationController __instance)
		{
			DebugLog.DebugWrite($"Start Collapse");
			return true;
		}
	}
}
