using OWML.ModHelper.Events;
using QSB.Animation;
using QSB.Events;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerTransformSync : TransformSync
    {
        public static PlayerTransformSync LocalInstance { get; private set; }

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        private Transform GetPlayerModel()
        {
            return Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2");
        }

        protected override Transform InitLocalTransform()
        {
            var body = GetPlayerModel();

            GetComponent<AnimationSync>().InitLocal(body);

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            var body = Instantiate(GetPlayerModel());

            GetComponent<AnimationSync>().InitRemote(body);

            var marker = body.gameObject.AddComponent<PlayerHUDMarker>();
            marker.SetId(netId.Value);

            var flashlightRoot = Instantiate(GameObject.Find("FlashlightRoot"));
            flashlightRoot.SetActive(false);
            var oldComponent = flashlightRoot.GetComponent<Flashlight>();
            var component = flashlightRoot.AddComponent<QSBFlashlight>();
            component._lights = oldComponent.GetValue<OWLight2[]>("_lights");
            component._illuminationCheckLight = oldComponent.GetValue<OWLight2>("_illuminationCheckLight");
            component._root = oldComponent.GetValue<Transform>("_root");
            component._basePivot = oldComponent.GetValue<Transform>("_basePivot");
            component._wobblePivot = oldComponent.GetValue<Transform>("_wobblePivot");
            oldComponent.enabled = false;
            flashlightRoot.transform.parent = body;
            flashlightRoot.SetActive(true);

            Finder.RegisterPlayer(netId.Value, body.gameObject);

            return body;
        }

        protected override bool IsReady()
        {
            return Locator.GetPlayerTransform() != null;
        }

    }
}
