using HarmonyLib;
using QSB.EyeOfTheUniverse.InstrumentSync.Messages;
using QSB.EyeOfTheUniverse.InstrumentSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.InstrumentSync.Patches
{
	internal class QuantumInstrumentPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(QuantumInstrument), nameof(QuantumInstrument.Gather))]
		public static void Gather(QuantumInstrument __instance)
			=> QSBWorldSync.GetWorldFromUnity<QSBQuantumInstrument>(__instance).SendMessage(new GatherInstrumentMessage());
	}
}
