using HarmonyLib;
using OWML.Common;
using QSB.Patches;
using QSB.Utility;
using System;
using UnityEngine;

namespace QSB;

/// <summary>
/// TEMPORARY: this is for trying to solve this stupid fucking bug (gorp)
/// </summary>
[HarmonyPatch(typeof(OWRigidbody))]
public class TeleportingPlanetsPatch : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

	[HarmonyPostfix]
	[HarmonyPatch(nameof(OWRigidbody.SetPosition))]
	private static void SetPosition(OWRigidbody __instance, Vector3 worldPosition)
	{
		if (__instance.TryGetComponent<AstroObject>(out var astroObject) && astroObject._name != AstroObject.Name.ProbeCannon)
		{
			DebugLog.ToAll($"AHHHHHHHHH!!!!!!!!!\n{__instance.name}\n{Environment.StackTrace}", MessageType.Error);
		}
	}
}
