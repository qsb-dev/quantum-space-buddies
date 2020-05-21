using OWML.ModHelper.Events;
using System.Reflection;
using UnityEngine;

namespace Marshmallow.Atmosphere
{
    static class MakeBaseEffects
    {
        public static void Make(GameObject body)
        {
            GameObject main = new GameObject();
            main.SetActive(false);
            main.transform.parent = body.transform;

            SectorCullGroup maincull = main.AddComponent<SectorCullGroup>();
            maincull.SetValue("_sector", Main.SECTOR);
            maincull.SetValue("_particleSystemSuspendMode", CullGroup.ParticleSystemSuspendMode.Stop);
            maincull.SetValue("_occlusionCulling", false);
            maincull.SetValue("_dynamicCullingBounds", false);
            maincull.SetValue("_waitForStreaming", false);

            GameObject rain = new GameObject();
            rain.SetActive(false);
            rain.transform.parent = main.transform;

            ParticleSystem ps = GameObject.Instantiate(GameObject.Find("Effects_GD_Rain").GetComponent<ParticleSystem>());

            VectionFieldEmitter vfe = rain.AddComponent<VectionFieldEmitter>();
            vfe.fieldRadius = 20f;
            vfe.particleCount = 10;
            vfe.emitOnLeadingEdge = false;
            vfe.emitDirection = VectionFieldEmitter.EmitDirection.Radial;
            vfe.reverseDir = true;
            vfe.SetValue("_affectingForces", new ForceVolume[0]);
            vfe.SetValue("_applyForcePerParticle", false);

            PlanetaryVectionController pvc = rain.AddComponent<PlanetaryVectionController>();
            pvc.SetValue("_followTarget", pvc.GetType().GetNestedType("FollowTarget", BindingFlags.NonPublic).GetField("Player").GetValue(pvc));
            pvc.SetValue("_activeInSector", Main.SECTOR);

            rain.GetComponent<Renderer>().material = GameObject.Find("Effects_GD_Rain").GetComponent<Renderer>().material;

            main.SetActive(true);
            rain.SetActive(true);
        }
    }
}
