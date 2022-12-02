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
		if (__instance.GetComponent<AstroObject>())
		{
			DebugLog.ToAll($"AHHHHHHHHH!!!!!!!!!\n{Environment.StackTrace}", MessageType.Error);
		}
	}
}
