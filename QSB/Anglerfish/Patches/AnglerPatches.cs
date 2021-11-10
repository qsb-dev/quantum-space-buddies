using HarmonyLib;
using QSB.Anglerfish.WorldObjects;
using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Anglerfish.Patches
{
	public class AnglerPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.GetTargetPosition))]
		public static bool GetTargetPosition(AnglerfishController __instance, ref Vector3 __result)
		{
			var target = QSBWorldSync.GetWorldFromUnity<QSBAngler>(__instance).target;
			__result = target != null ? target.position : __instance._localDisturbancePos;
			return false;
		}
	}
}
