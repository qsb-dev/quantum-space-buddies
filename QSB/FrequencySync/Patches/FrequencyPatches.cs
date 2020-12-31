using QSB.Events;
using QSB.Patches;

namespace QSB.FrequencySync.Patches
{
	public class FrequencyPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			QSBCore.Helper.HarmonyHelper.AddPostfix<AudioSignal>("IdentifyFrequency", typeof(FrequencyPatches), nameof(IdentifyFrequency));
			QSBCore.Helper.HarmonyHelper.AddPostfix<AudioSignal>("IdentifySignal", typeof(FrequencyPatches), nameof(IdentifySignal));
		}

		public static void IdentifyFrequency(SignalFrequency ____frequency)
			=> GlobalMessenger<SignalFrequency>.FireEvent(EventNames.QSBIdentifyFrequency, ____frequency);

		public static void IdentifySignal(SignalName ____name)
			=> GlobalMessenger<SignalName>.FireEvent(EventNames.QSBIdentifySignal, ____name);
	}
}
