using OWML.ModHelper.Events;
using UnityEngine;

namespace Marshmallow.Atmosphere
{
    static class MakeSunOverride
    {
        public static void Make(GameObject body, float topCloudScale, float bottomCloudScale, float waterSize)
        {
            GameObject sunov = new GameObject();
            sunov.SetActive(false);
            sunov.transform.parent = body.transform;

            GiantsDeepSunOverrideVolume vol = sunov.AddComponent<GiantsDeepSunOverrideVolume>();
            vol.SetValue("_sector", Main.SECTOR);
            vol.SetValue("_cloudsOuterRadius", topCloudScale / 2);
            vol.SetValue("_cloudsInnerRadius", bottomCloudScale / 2);
            vol.SetValue("_waterOuterRadius", waterSize / 2);
            vol.SetValue("_waterInnerRadius", 402.5f);

            sunov.SetActive(true);
        }
    }
}
