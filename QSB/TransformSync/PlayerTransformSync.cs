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

        public Transform bodyTransform;

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
            DebugLog.ToConsole("Local for player " + netId.Value);
            var body = GetPlayerModel();

            bodyTransform = body;

            GetComponent<AnimationSync>().InitLocal(body);

            Finder.RegisterPlayerBody(netId.Value, body.gameObject);

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            DebugLog.ToConsole("Remote for player " + netId.Value);
            var body = Instantiate(GetPlayerModel());

            bodyTransform = body;

            GetComponent<AnimationSync>().InitRemote(body);

            var marker = body.gameObject.AddComponent<PlayerHUDMarker>();
            marker.SetId(netId.Value);

            Finder.RegisterPlayerBody(netId.Value, body.gameObject);

            return body;
        }

        protected override bool IsReady()
        {
            return Locator.GetPlayerTransform() != null;
        }
    }
}
