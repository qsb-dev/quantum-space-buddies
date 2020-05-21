using OWML.ModHelper.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Marshmallow.General
{
    static class MakeOrbitLine
    {
        public static void Make(GameObject body, AstroObject astroobject)
        {
            GameObject orbit = new GameObject();
            orbit.transform.parent = body.transform;

            orbit.AddComponent<LineRenderer>();

            var ol = orbit.AddComponent<OrbitLine>();
            ol.SetValue("_astroObject", astroobject);
            ol.SetValue("_fade", false);
        }
    }
}
