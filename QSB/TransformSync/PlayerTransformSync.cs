using OWML.ModHelper.Events;
using QSB.Animation;
using QSB.Events;
using QSB.Utility;
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

        protected override Transform InitLocalTransform()
        {
            var body = Locator.GetPlayerTransform();

            GetComponent<AnimationSync>().InitLocal(body.Find("Traveller_HEA_Player_v2"));

            PlayerToolsManager.Init(body, true);

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            var body = Instantiate(Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2"));

            var root = new GameObject("Player_Body");
            body.parent = root.transform;

            GetComponent<AnimationSync>().InitRemote(root.transform);

            var marker = root.AddComponent<PlayerHUDMarker>();
            marker.SetId(netId.Value);

            PlayerToolsManager.Init(root.transform, false);

            Finder.RegisterPlayer(netId.Value, root);

            return root.transform;
        }

        protected override bool IsReady()
        {
            return Locator.GetPlayerTransform() != null;
        }
    }
}
