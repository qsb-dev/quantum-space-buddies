using QSB.Events;
using UnityEngine.Networking;

namespace QSB.Utility
{
    public static class Patches
    {
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

        public static void DoPatches()
        {
            QSB.Helper.HarmonyHelper.AddPostfix<SurveyorProbe>("OnAnchor", typeof(Patches), nameof(ProbeAnchor));
            QSB.Helper.HarmonyHelper.AddPrefix<SurveyorProbe>("Retrieve", typeof(Patches), nameof(ProbeWarp));
            QSB.Helper.HarmonyHelper.EmptyMethod<NetworkManagerHUD>("Update");
        }
    }
}
