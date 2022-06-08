using HarmonyLib;
using QSB.Messaging;
using QSB.ModelShip.Messages;
using QSB.Patches;

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
}
