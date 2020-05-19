using OWML.ModHelper.Events;
using QSB.Animation;
using QSB.Events;
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

            return body;
        }

        protected override bool IsReady()
        {
            return Locator.GetPlayerTransform() != null;
        }

    }
}
