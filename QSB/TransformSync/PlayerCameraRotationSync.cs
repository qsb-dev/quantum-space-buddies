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
        protected override Transform InitLocalTransform()
        {
            var body = Locator.GetPlayerTransform().Find("PlayerCamera");

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            var body = gameObject.transform.Find("PlayerCamera");

            return body.transform;
        }

        protected override bool IsReady()
        {
            return Locator.GetPlayerTransform() != null;
        }
    }
}
