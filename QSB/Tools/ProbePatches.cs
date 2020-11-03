using QSB.Events;

namespace QSB.Tools
{
    public class ProbePatches : QSBPatch
    {
        public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

        private static void ProbeAnchor()
        {
            GlobalMessenger.FireEvent(EventNames.QSBOnProbeAnchor);
        }

        private static bool ProbeWarp(ref bool ____isRetrieving)
        {
            if (!____isRetrieving)
            {
                GlobalMessenger.FireEvent(EventNames.QSBOnProbeWarp);
            }
            return true;
        }

        public override void DoPatches()
        {
            QSB.Helper.HarmonyHelper.AddPostfix<SurveyorProbe>("OnAnchor", typeof(ProbePatches), nameof(ProbeAnchor));
            QSB.Helper.HarmonyHelper.AddPrefix<SurveyorProbe>("Retrieve", typeof(ProbePatches), nameof(ProbeWarp));
        }
    }
}
