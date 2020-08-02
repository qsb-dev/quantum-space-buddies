using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.Utility
{
    public static class Patches
    {
        private static void ProbeAnchor()
        {
            GlobalMessenger.FireEvent("QSBProbeAnchor");
        }

        private static bool ProbeWarp(ref bool ____isRetrieving)
        {
            if (!____isRetrieving)
            {
                GlobalMessenger.FireEvent("QSBOnProbeWarp");
            }  
            return true;
        }

        public static void DoPatches(IHarmonyHelper helper)
        {
            helper.AddPostfix<SurveyorProbe>("OnAnchor", typeof(Patches), nameof(ProbeAnchor));
            helper.AddPrefix<SurveyorProbe>("Retrieve", typeof(Patches), nameof(ProbeWarp));
        }
    }
}
