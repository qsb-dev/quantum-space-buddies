using OWML.ModHelper.Events;
using QSB.Animation;
using QSB.Events;
using QSB.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerCameraRotationSync : RotationSync
    {
        private Transform GetPlayerModel()
        {
            return Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2");
        }

        protected override Transform InitLocalTransform()
        {
            var camera = Locator.GetPlayerCamera().gameObject;

            PlayerToolsManager.Init(camera.transform);

            return camera.transform;
        }

        protected override Transform InitRemoteTransform()
        {
            var temp = gameObject.GetComponentInParent<PlayerTransformSync>().bodyTransform;

            var camera = new GameObject("PlayerCamera");
            camera.transform.parent = temp.transform;
            camera.transform.localPosition = new Vector3(0, 0.8496093f, 0.15f);

            PlayerToolsManager.Init(camera.transform);

            return camera.transform;
        }

        protected override bool IsReady()
        {
            return Locator.GetPlayerTransform() != null;
        }
    }
}
