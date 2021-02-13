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

		public override void DoUnpatches()
		{
			QSBCore.Helper.HarmonyHelper.Unpatch<AudioSignal>("IdentifyFrequency");
			QSBCore.Helper.HarmonyHelper.Unpatch<AudioSignal>("IdentifySignal");
		}

		public static void IdentifyFrequency(SignalFrequency ____frequency)
			=> QSBEventManager.FireEvent<SignalFrequency>(EventNames.QSBIdentifyFrequency, ____frequency);

		public static void IdentifySignal(SignalName ____name)
			=> QSBEventManager.FireEvent<SignalName>(EventNames.QSBIdentifySignal, ____name);
	}
}
