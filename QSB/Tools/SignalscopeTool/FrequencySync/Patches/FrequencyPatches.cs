using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.Tools.SignalscopeTool.FrequencySync.Messages;

namespace QSB.Tools.SignalscopeTool.FrequencySync.Patches
{
	[HarmonyPatch]
	public class FrequencyPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(AudioSignal), nameof(AudioSignal.IdentifyFrequency))]
		public static void IdentifyFrequencyEvent(AudioSignal __instance)
			=> new IdentifyFrequencyMessage(__instance._frequency).Send();

		[HarmonyPostfix]
		[HarmonyPatch(typeof(AudioSignal), nameof(AudioSignal.IdentifySignal))]
		public static void IdentifySignalEvent(AudioSignal __instance)
			=> new IdentifySignalMessage(__instance._name).Send();
	}
}