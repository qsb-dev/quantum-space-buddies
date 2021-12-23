using HarmonyLib;
using QSB.Events;
using QSB.EyeOfTheUniverse.InstrumentSync.WorldObjects;
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
			=> QSBEventManager.FireEvent(EventNames.QSBGatherInstrument, QSBWorldSync.GetWorldFromUnity<QSBQuantumInstrument>(__instance));
	}
}
