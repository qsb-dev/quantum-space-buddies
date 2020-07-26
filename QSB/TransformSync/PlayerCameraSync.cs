using OWML.ModHelper.Events;
using QSB.Animation;
using QSB.Events;
using QSB.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerCameraSync : TransformSync
    {
        public static PlayerCameraSync LocalInstance { get; private set; }

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        protected override Transform InitLocalTransform()
        {
            DebugLog.ToConsole("Local for camera " + PlayerTransformSync.LocalInstance.netId.Value);
            var body = Locator.GetPlayerCamera().gameObject.transform;

            PlayerToolsManager.Init(body);

            Finder.RegisterPlayerCamera(PlayerTransformSync.LocalInstance.netId.Value, body.gameObject);

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            DebugLog.ToConsole("Remote for camera " + PlayerTransformSync.LocalInstance.netId.Value);
            var body = new GameObject("PlayerCamera");

            PlayerToolsManager.Init(body.transform);

            Finder.RegisterPlayerCamera(PlayerTransformSync.LocalInstance.netId.Value, body);

            return body.transform;
        }

        protected override bool IsReady()
        {
            return Locator.GetPlayerTransform() != null;
        }
    }
}
