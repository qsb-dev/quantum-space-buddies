using OWML.ModHelper.Events;
using UnityEngine;

namespace Marshmallow.General
{
    static class MakeRFVolume
    {
        public static void Make(GameObject body)
        {
            GameObject RFVolume = new GameObject();
            RFVolume.name = "RFVolume";
            RFVolume.transform.parent = body.transform;
            RFVolume.layer = 19;
            RFVolume.SetActive(false);

            SphereCollider RF_SC = RFVolume.AddComponent<SphereCollider>();
            RF_SC.isTrigger = true;
            RF_SC.radius = 600f;

            ReferenceFrameVolume RF_RFV = RFVolume.AddComponent<ReferenceFrameVolume>();
            ReferenceFrame test = new ReferenceFrame(Main.OWRB);
            test.SetValue("_minSuitTargetDistance", 300);
            test.SetValue("_maxTargetDistance", 0);
            test.SetValue("_autopilotArrivalDistance", 1000);
            test.SetValue("_autoAlignmentDistance", 1000);
            test.SetValue("_hideLandingModePrompt", false);
            test.SetValue("_matchAngularVelocity", true);
            test.SetValue("_minMatchAngularVelocityDistance", 70);
            test.SetValue("_maxMatchAngularVelocityDistance", 400);
            test.SetValue("_bracketsRadius", 300);
            RF_RFV.SetValue("_referenceFrame", test);
            RF_RFV.SetValue("_minColliderRadius", 300);
            RF_RFV.SetValue("_maxColliderRadius", 2000);
            RF_RFV.SetValue("_isPrimaryVolume", true);
            RF_RFV.SetValue("_isCloseRangeVolume", false);
            RFVolume.SetActive(true);
        }
    }
}
