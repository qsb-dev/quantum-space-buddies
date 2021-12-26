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
		public static void IdentifyFrequencyEvent(SignalFrequency ____frequency)
			=> new IdentifyFrequencyMessage(____frequency).Send();

		[HarmonyPostfix]
		[HarmonyPatch(typeof(AudioSignal), nameof(AudioSignal.IdentifySignal))]
		public static void IdentifySignalEvent(SignalName ____name)
			=> new IdentifySignalMessage(____name).Send();
	}
}
