using OWML.ModHelper.Events;
using UnityEngine;

namespace Marshmallow.Atmosphere
{
    static class MakeAir
    {
        public static void Make(GameObject body, float airScale, bool isRaining)
        {
            GameObject air = new GameObject();
            air.layer = 17;
            air.SetActive(false);
            air.transform.parent = body.transform;

            SphereCollider atmoSC = air.AddComponent<SphereCollider>();
            atmoSC.isTrigger = true;
            atmoSC.radius = airScale;

            SimpleFluidVolume sfv = air.AddComponent<SimpleFluidVolume>();
            sfv.SetValue("_layer", 5);
            sfv.SetValue("_priority", 1);
            sfv.SetValue("_density", 1.2f);
            sfv.SetValue("_fluidType", FluidVolume.Type.AIR);
            sfv.SetValue("_allowShipAutoroll", true);
            sfv.SetValue("_disableOnStart", false);

            if (isRaining)
            {
                VisorRainEffectVolume vref = air.AddComponent<VisorRainEffectVolume>();
                vref.SetValue("_rainDirection", VisorRainEffectVolume.RainDirection.Radial);
                vref.SetValue("_layer", 0);
                vref.SetValue("_priority", 0);

                AudioSource auds = air.AddComponent<AudioSource>();
                auds.mute = false;
                auds.bypassEffects = false;
                auds.bypassListenerEffects = false;
                auds.bypassReverbZones = false;
                auds.playOnAwake = false;
                auds.loop = true;
                auds.priority = 128;
                auds.volume = 0.35f;
                auds.pitch = 1f;
                auds.panStereo = 0f;
                auds.spatialBlend = 0f;
                auds.reverbZoneMix = 1f;

                OWAudioSource owas = air.AddComponent<OWAudioSource>();
                owas.SetAudioLibraryClip(AudioType.GD_RainAmbient_LP);
                owas.SetClipSelectionType(OWAudioSource.ClipSelectionOnPlay.RANDOM);
                owas.SetTrack(OWAudioMixer.TrackName.Environment);

                AudioVolume av = air.AddComponent<AudioVolume>();
            }

            air.SetActive(true);
        }
    }
}
