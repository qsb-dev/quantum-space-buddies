using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Marshmallow
{
    public class Main
    {
        public static OWRigidbody OWRB;
        public static Sector SECTOR;
        public static SpawnPoint SPAWN;
        public static AstroObject ASTROOBJECT;

        public static IModHelper helper;

        static List<PlanetConfig> planetList = new List<PlanetConfig>();

        public static void CreatePlanet()
        {
            var planet = GenerateBody();

            planet.transform.parent = Locator.GetRootTransform();

            planet.transform.position = new Vector3(10000, 0, 10000);
            planet.SetActive(true);

            General.MakeOrbitLine.Make(planet, ASTROOBJECT);
        }

        private static GameObject GenerateBody()
        {
            var name = "Player Planet";
            var orbitAngle = 0.01f;
            var hasGravity = true;
            var surfaceAcceleration = 12;
            var hasMapMarker = true;
            var hasClouds = true;
            var cloudTint = new Color32(255, 0, 0, 128);
            var fogTint = new Color32(0, 75, 15, 128);
            var waterSize = 401f;
            var hasRain = true;
            var hasWater = true;
            var hasFog = false;
            var fogDensity = 0.75f;
            var makeSpawnPoint = true;
            var groundScale = 400f;
            var topCloudSize = 650f;
            var bottomCloudSize = 600f;

            QSB.DebugLog.Console("Begin generation sequence of planet [" + name + "] ...");


            QSB.DebugLog.Console("Got top cloud size as " + topCloudSize);
            QSB.DebugLog.Console("Got bottom cloud size as " + bottomCloudSize);

            GameObject body;

            body = new GameObject(name);
            body.SetActive(false);

            Body.MakeGeometry.Make(body, groundScale);

            ASTROOBJECT = General.MakeOrbitingAstroObject.Make(body, 0.02f, orbitAngle, hasGravity, surfaceAcceleration, groundScale);
            General.MakeRFVolume.Make(body);

            if (hasMapMarker)
            {
                General.MakeMapMarker.Make(body, name);
            }

            SECTOR = Body.MakeSector.Make(body, topCloudSize);

            if (hasClouds)
            {
                Atmosphere.MakeClouds.Make(body, topCloudSize, bottomCloudSize, cloudTint);
                Atmosphere.MakeSunOverride.Make(body, topCloudSize, bottomCloudSize, waterSize);
            }

            Atmosphere.MakeAir.Make(body, topCloudSize / 2, hasRain);

            if (hasWater)
            {
                Body.MakeWater.Make(body, waterSize);
            }

            Atmosphere.MakeBaseEffects.Make(body);
            Atmosphere.MakeVolumes.Make(body, groundScale, topCloudSize);
            General.MakeAmbientLight.Make(body);
            Atmosphere.MakeAtmosphere.Make(body, topCloudSize, hasFog, fogDensity, fogTint);

            if (makeSpawnPoint)
            {
                SPAWN = General.MakeSpawnPoint.Make(body, new Vector3(0, groundScale + 10, 0));
            }

            QSB.DebugLog.Console("Generation of planet [" + name + "] completed.");

            return body;
        }
    }
}
