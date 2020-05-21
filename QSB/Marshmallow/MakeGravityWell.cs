using OWML.ModHelper.Events;
using System.Reflection;
using UnityEngine;

namespace Marshmallow.General
{
    static class MakeGravityWell
    {
        public static GravityVolume Make(GameObject body, float surfaceAccel, float upperSurface, float lowerSurface)
        {
            GameObject GravityWell = new GameObject();
            GravityWell.transform.parent = body.transform;
            GravityWell.name = "GravityWell";
            GravityWell.layer = 17;
            GravityWell.SetActive(false);

            GravityVolume GV = GravityWell.AddComponent<GravityVolume>();
            GV.SetValue("_cutoffAcceleration", 0.1f);
            GV.SetValue("_falloffType", GV.GetType().GetNestedType("FalloffType", BindingFlags.NonPublic).GetField("linear").GetValue(GV));
            GV.SetValue("_alignmentRadius", 600f);
            GV.SetValue("_upperSurfaceRadius", upperSurface);
            GV.SetValue("_lowerSurfaceRadius", lowerSurface);
            GV.SetValue("_layer", 3);
            GV.SetValue("_priority", 0);
            GV.SetValue("_alignmentPriority", 0);
            GV.SetValue("_surfaceAcceleration", surfaceAccel);
            GV.SetValue("_inheritable", false);
            GV.SetValue("_isPlanetGravityVolume", true);
            GV.SetValue("_cutoffRadius", 55f);

            SphereCollider GV_SC = GravityWell.AddComponent<SphereCollider>();
            GV_SC.isTrigger = true;
            GV_SC.radius = 4000;

            OWCollider GV_OWC = GravityWell.AddComponent<OWCollider>();
            GV_OWC.SetLODActivationMask(DynamicOccupant.Player);
            GravityWell.SetActive(true);
            return GV;
        }
    }
}
