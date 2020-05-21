using OWML.ModHelper.Events;
using System.Collections.Generic;
using UnityEngine;

namespace Marshmallow.Body
{
    static class MakeSector
    {
        public static Sector Make(GameObject body, float sectorSize)
        {
            GameObject sectorBase = new GameObject();
            sectorBase.SetActive(false);
            sectorBase.transform.parent = body.transform;

            SphereShape sphereshape = sectorBase.AddComponent<SphereShape>();
            sphereshape.SetCollisionMode(Shape.CollisionMode.Volume);
            sphereshape.SetLayer(Shape.Layer.Sector);
            sphereshape.layerMask = -1;
            sphereshape.pointChecksOnly = true;
            sphereshape.radius = 700f;
            sphereshape.center = Vector3.zero;

            OWTriggerVolume trigVol = sectorBase.AddComponent<OWTriggerVolume>();

            Sector sector = sectorBase.AddComponent<Sector>();
            sector.SetValue("_name", Sector.Name.InvisiblePlanet);
            sector.SetValue("__attachedOWRigidbody", Main.OWRB);
            sector.SetValue("_subsectors", new List<Sector>());

            sectorBase.SetActive(true);

            return sector;
        }
    }
}
