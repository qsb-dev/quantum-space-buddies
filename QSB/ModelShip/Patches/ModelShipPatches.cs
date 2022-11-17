using HarmonyLib;
using QSB.Messaging;
using QSB.ModelShip.Messages;
using QSB.Patches;
using UnityEngine;

namespace QSB.ModelShip.Patches;

public class ModelShipPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(RemoteFlightConsole), nameof(RemoteFlightConsole.RespawnModelShip))]
	private static void RemoteFlightConsole_RespawnModelShip(bool playEffects)
	{
		if (Remote)
		{
			return;
		}

		new RespawnModelShipMessage(playEffects).Send();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ModelShipCrashBehavior), nameof(ModelShipCrashBehavior.OnImpact))]
	private static void ModelShipCrashBehavior_OnImpact(ModelShipCrashBehavior __instance, ImpactData impactData)
	{
		if (impactData.speed > 10f && Time.time > __instance._lastCrashTime + 1f)
		{
			new CrashModelShipMessage().Send();
		}
	}
}
