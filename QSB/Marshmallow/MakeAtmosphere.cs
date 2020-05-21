using UnityEngine;

namespace Marshmallow.Atmosphere
{
    static class MakeAtmosphere
    {
        public static void Make(GameObject body, float topCloudScale, bool hasFog, float fogDensity, Color fogTint)
        {
            topCloudScale /= 2;

            GameObject atmoM = new GameObject();
            atmoM.SetActive(false);
            atmoM.name = "Atmosphere";
            atmoM.transform.parent = body.transform;

            if (hasFog)
            {
                GameObject fog = new GameObject();
                fog.SetActive(false);
                fog.name = "FogSphere";
                fog.transform.parent = atmoM.transform;
                fog.transform.localScale = new Vector3(topCloudScale + 10, topCloudScale + 10, topCloudScale + 10);

                MeshFilter mf = fog.AddComponent<MeshFilter>();
                mf.mesh = GameObject.Find("Atmosphere_GD/FogSphere").GetComponent<MeshFilter>().mesh;

                MeshRenderer mr = fog.AddComponent<MeshRenderer>();
                mr.materials = GameObject.Find("Atmosphere_GD/FogSphere").GetComponent<MeshRenderer>().materials;
                mr.allowOcclusionWhenDynamic = true;

                PlanetaryFogController pfc = fog.AddComponent<PlanetaryFogController>();
                pfc.fogLookupTexture = GameObject.Find("Atmosphere_GD/FogSphere").GetComponent<PlanetaryFogController>().fogLookupTexture;
                pfc.fogRadius = topCloudScale + 10;
                pfc.fogDensity = fogDensity;
                pfc.fogExponent = 1f;
                pfc.fogColorRampTexture = GameObject.Find("Atmosphere_GD/FogSphere").GetComponent<PlanetaryFogController>().fogColorRampTexture;
                pfc.fogColorRampIntensity = 1f;
                pfc.fogTint = fogTint;

                fog.SetActive(true);
            }

            /*
            GameObject atmo = new GameObject();
            atmo.SetActive(false);
            atmo.transform.parent = atmoM.transform;
            atmo.transform.localScale = new Vector3(topCloudScale + 100, topCloudScale + 100, topCloudScale + 100);

            Material mat = GameObject.Find("Atmosphere_LOD0").GetComponent<MeshRenderer>().material;

            GameObject lod0 = new GameObject();
            lod0.transform.parent = atmo.transform;
            MeshFilter f0 = lod0.AddComponent<MeshFilter>();
            f0.mesh = GameObject.Find("Atmosphere_LOD0").GetComponent<MeshFilter>().mesh;
            MeshRenderer r0 = lod0.AddComponent<MeshRenderer>();
            r0.material = mat;

            GameObject lod1 = new GameObject();
            lod0.transform.parent = atmo.transform;
            MeshFilter f1 = lod1.AddComponent<MeshFilter>();
            f1.mesh = GameObject.Find("Atmosphere_LOD1").GetComponent<MeshFilter>().mesh;
            MeshRenderer r1 = lod1.AddComponent<MeshRenderer>();
            r1.material = mat;

            GameObject lod2 = new GameObject();
            lod2.transform.parent = atmo.transform;
            MeshFilter f2 = lod2.AddComponent<MeshFilter>();
            f2.mesh = GameObject.Find("Atmosphere_LOD2").GetComponent<MeshFilter>().mesh;
            MeshRenderer r2 = lod2.AddComponent<MeshRenderer>();
            r2.material = mat;

            GameObject lod3 = new GameObject();
            lod3.transform.parent = atmo.transform;
            MeshFilter f3 = lod3.AddComponent<MeshFilter>();
            f3.mesh = GameObject.Find("Atmosphere_LOD3").GetComponent<MeshFilter>().mesh;
            MeshRenderer r3 = lod3.AddComponent<MeshRenderer>();
            r3.material = mat;

            // THIS FUCKING THING. do NOT ask why i have done this. IT WORKS.
            // This creates an LOD group in the worst way possible. i am so sorry.
            LODGroup lodg = atmo.AddComponent<LODGroup>();
            
            LOD[] lodlist = new LOD[4];
            Renderer[] t0 = { r0 };
            Renderer[] t1 = { r1 };
            Renderer[] t2 = { r2 };
            Renderer[] t3 = { r3 };
            LOD one = new LOD(1, t0);
            LOD two = new LOD(0.7f, t1);
            LOD three = new LOD(0.27f, t2);
            LOD four = new LOD(0.08f, t3);
            lodlist[0] = one;
            lodlist[1] = two;
            lodlist[2] = three;
            lodlist[3] = four;

            lodg.SetLODs(lodlist);
            lodg.fadeMode = LODFadeMode.None;
            */

            //atmo.SetActive(true);
            atmoM.SetActive(true);
        }
    }
}
