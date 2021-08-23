using QSB.Events;
using QSB.Patches;

namespace QSB.FrequencySync.Patches
{
	public class FrequencyPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			Postfix(nameof(AudioSignal_IdentifyFrequency));
			Postfix(nameof(AudioSignal_IdentifySignal));
		}

		public static void AudioSignal_IdentifyFrequency(SignalFrequency ____frequency)
			=> QSBEventManager.FireEvent(EventNames.QSBIdentifyFrequency, ____frequency);

		public static void AudioSignal_IdentifySignal(SignalName ____name)
			=> QSBEventManager.FireEvent(EventNames.QSBIdentifySignal, ____name);
	}
}
