using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.Utility
{
    public static class Patches
    {
        private static void OnAnchor()
        {
            GlobalMessenger.FireEvent("QSBProbeAnchor");
        }

        public static void DoPatches(IHarmonyHelper helper)
        {
            helper.AddPostfix<SurveyorProbe>("OnAnchor", typeof(Patches), nameof(OnAnchor));
        }
    }
}
