using OWML.ModHelper.Events;
using UnityEngine;

namespace Marshmallow.General
{
    static class MakeFieldDetector
    {
        public static void Make(GameObject body)
        {
            GameObject FieldDetector = new GameObject();
            FieldDetector.SetActive(false);
            FieldDetector.name = "FieldDetector";
            FieldDetector.transform.parent = body.transform;
            FieldDetector.layer = 20;

            ConstantForceDetector CFD = FieldDetector.AddComponent<ConstantForceDetector>();
            ForceVolume[] temp = new ForceVolume[1];
            temp[0] = Locator.GetAstroObject(AstroObject.Name.Sun).GetGravityVolume();
            CFD.SetValue("_detectableFields", temp);
            CFD.SetValue("_inheritElement0", false);
            FieldDetector.SetActive(true);
        }
    }
}
