using OWML.ModHelper.Events;
using UnityEngine;

namespace Marshmallow.Body
{
    static class MakeWater
    {
        public static void Make(GameObject body, float waterScale)
        {
            GameObject waterBase = new GameObject();
            waterBase.SetActive(false);
            waterBase.layer = 15;
            waterBase.transform.parent = body.transform;
            waterBase.transform.localScale = new Vector3(waterScale / 2, waterScale / 2, waterScale / 2);
            waterBase.DestroyAllComponents<SphereCollider>();

            TessellatedSphereRenderer tsr = waterBase.AddComponent<TessellatedSphereRenderer>();
            tsr.tessellationMeshGroup = GameObject.Find("Ocean_GD").GetComponent<TessellatedSphereRenderer>().tessellationMeshGroup;
            tsr.sharedMaterials = GameObject.Find("Ocean_GD").GetComponent<TessellatedSphereRenderer>().sharedMaterials;
            tsr.maxLOD = 7;
            tsr.LODBias = 2;
            tsr.LODRadius = 2f;

            TessSphereSectorToggle toggle = waterBase.AddComponent<TessSphereSectorToggle>();
            toggle.SetValue("_sector", Main.SECTOR);

            OceanEffectController effectC = waterBase.AddComponent<OceanEffectController>();
            effectC.SetValue("_sector", Main.SECTOR);
            effectC.SetValue("_ocean", tsr);

            // Because assetbundles were a bitch...

            GameObject fog1 = new GameObject();
            fog1.transform.parent = waterBase.transform;
            fog1.transform.localScale = new Vector3(1, 1, 1);
            fog1.AddComponent<MeshFilter>().mesh = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;
            fog1.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Sprites/Default"));
            fog1.GetComponent<MeshRenderer>().material.color = new Color32(0, 75, 50, 5);

            GameObject fog2 = new GameObject();
            fog2.transform.parent = waterBase.transform;
            fog2.transform.localScale = new Vector3(1.001f, 1.001f, 1.001f);
            fog2.AddComponent<MeshFilter>().mesh = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;
            fog2.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Sprites/Default"));
            fog2.GetComponent<MeshRenderer>().material.color = new Color32(0, 75, 50, 5);

            GameObject fog3 = new GameObject();
            fog3.transform.parent = fog2.transform;
            fog3.transform.localScale = new Vector3(1.001f, 1.001f, 1.001f);
            fog3.AddComponent<MeshFilter>().mesh = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;
            fog3.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Sprites/Default"));
            fog3.GetComponent<MeshRenderer>().material.color = new Color32(0, 75, 50, 5);

            waterBase.SetActive(true);

            //QSB.DebugLog.Console("Water - waterScale : " + waterScale);
        }
    }
}
