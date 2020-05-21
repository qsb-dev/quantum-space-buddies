using OWML.ModHelper.Events;
using UnityEngine;

namespace Marshmallow.Atmosphere
{
    static class MakeClouds
    {
        public static void Make(GameObject body, float topCloudScale, float bottomCloudScale, Color? cloudTint = null)
        {
            GameObject cloudsMain = new GameObject();
            cloudsMain.SetActive(false);
            cloudsMain.transform.parent = body.transform;

            GameObject cloudsTop = new GameObject();
            cloudsTop.SetActive(false);
            cloudsTop.transform.parent = cloudsMain.transform;
            cloudsTop.transform.localScale = new Vector3(topCloudScale / 2, topCloudScale / 2, topCloudScale / 2);

            MeshFilter MF = cloudsTop.AddComponent<MeshFilter>();
            MF.mesh = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;

            MeshRenderer MR = cloudsTop.AddComponent<MeshRenderer>();
            MR.materials = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshRenderer>().materials;

            foreach (var item in MR.materials)
            {
                item.SetColor("_Color", cloudTint.Value);
            }

            RotateTransform RT = cloudsTop.AddComponent<RotateTransform>();
            RT.SetValue("_localAxis", Vector3.up);
            RT.SetValue("degreesPerSecond", 10);
            RT.SetValue("randomizeRotationRate", false);

            /*
            SectorCullGroup scg = cloudsTop.AddComponent<SectorCullGroup>();
            scg.SetValue("_sector", MainClass.SECTOR);
            scg.SetValue("_occlusionCulling", true);
            scg.SetValue("_dynamicCullingBounds", false);
            scg.SetValue("_particleSystemSuspendMode", CullGroup.ParticleSystemSuspendMode.Pause);
            scg.SetValue("_waitForStreaming", false);
            */

            GameObject cloudsBottom = new GameObject();
            cloudsBottom.SetActive(false);
            cloudsBottom.transform.parent = cloudsMain.transform;
            cloudsBottom.transform.localScale = new Vector3(bottomCloudScale / 2, bottomCloudScale / 2, bottomCloudScale / 2);

            TessellatedSphereRenderer TSR = cloudsBottom.AddComponent<TessellatedSphereRenderer>();
            TSR.tessellationMeshGroup = GameObject.Find("CloudsBottomLayer_GD").GetComponent<TessellatedSphereRenderer>().tessellationMeshGroup;
            TSR.sharedMaterials = GameObject.Find("CloudsBottomLayer_GD").GetComponent<TessellatedSphereRenderer>().sharedMaterials;

            foreach (var item in TSR.sharedMaterials)
            {
                item.SetColor("_Color", cloudTint.Value);
            }

            TSR.maxLOD = 6;
            TSR.LODBias = 0;
            TSR.LODRadius = 1f;

            TessSphereSectorToggle TSST = cloudsBottom.AddComponent<TessSphereSectorToggle>();
            TSST.SetValue("_sector", Main.SECTOR);

            GameObject cloudsFluid = new GameObject();
            cloudsFluid.layer = 17;
            cloudsFluid.SetActive(false);
            cloudsFluid.transform.parent = cloudsMain.transform;

            SphereCollider cloudSC = cloudsFluid.AddComponent<SphereCollider>();
            cloudSC.isTrigger = true;
            cloudSC.radius = topCloudScale / 2;

            OWShellCollider cloudShell = cloudsFluid.AddComponent<OWShellCollider>();
            cloudShell.SetValue("_innerRadius", bottomCloudScale);

            CloudLayerFluidVolume cloudLayer = cloudsFluid.AddComponent<CloudLayerFluidVolume>();
            cloudLayer.SetValue("_layer", 5);
            cloudLayer.SetValue("_priority", 1);
            cloudLayer.SetValue("_density", 1.2f);
            cloudLayer.SetValue("_fluidType", FluidVolume.Type.CLOUD);
            cloudLayer.SetValue("_allowShipAutoroll", true);
            cloudLayer.SetValue("_disableOnStart", false);

            cloudsTop.SetActive(true);
            cloudsBottom.SetActive(true);
            cloudsFluid.SetActive(true);
            cloudsMain.SetActive(true);

            //QSB.DebugLog.Console("Clouds - topCloudScale : " + topCloudScale + ", bottomCloudScale : " + bottomCloudScale + ", cloudTint : " + cloudTint);
        }
    }
}
